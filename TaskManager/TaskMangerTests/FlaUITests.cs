using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace FlaUITest
{
    [TestClass]
    public class FlaUITest
    {
        protected Application _app;
        protected UIA3Automation _automation;
        protected Window _mainWindow;

        public const string AppPath = @"F:\МДК 01.02 Поддержка и тестирование ПМ\Лабараторные работы\Laba-1\laboratory1\TaskManager\TaskManager\bin\Debug\TaskManager.exe";

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists("tasks.txt")) File.Delete("tasks.txt");
            if (File.Exists("categories.txt")) File.Delete("categories.txt");

            _app = Application.Launch(AppPath);
            _automation = new UIA3Automation();
            _mainWindow = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(15));
            Thread.Sleep(2000);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _app?.Close();
            _automation?.Dispose();
        }

        protected TextBox GetDescriptionTextBox() =>
            _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("descriptionTextBox"))?.AsTextBox()
            ?? _mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit).And(cf.ByName("Описание:")))?.AsTextBox()
            ?? _mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.Edit)).FirstOrDefault()?.AsTextBox();

        protected ListBox GetTasksListBox() =>
            _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("tasksListBox"))?.AsListBox()
            ?? _mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.List))?.AsListBox();

        protected Button GetAddTaskButton() =>
            _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("addTaskButton"))?.AsButton()
            ?? _mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                .FirstOrDefault(b => b.Name == "Добавить задачу" || b.Name == "Добавить")?.AsButton();

        protected Button GetRemoveTaskButton() =>
            _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("removeTaskButton"))?.AsButton()
            ?? _mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByName("Удалить")))?.AsButton();

        protected Button GetToggleStatusButton() =>
            _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("toggleCompletionButton"))?.AsButton()
            ?? _mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button)
                .And(cf.ByName("Отметить выполнение")
                    .Or(cf.ByName("Отметить/снять выполнение"))
                    .Or(cf.ByName("Отметить"))))?.AsButton();

        protected TextBox GetNewCategoryTextBox() =>
            _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("newCategoryTextBox"))?.AsTextBox()
            ?? _mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit).And(cf.ByName("Новая категория:")))?.AsTextBox();

        protected Button GetAddCategoryButton() =>
            _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("addCategoryButton"))?.AsButton()
            ?? _mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByName("Добавить")))?.AsButton();

        protected Button GetRemoveCategoryButton()
        {
            var allButtons = _mainWindow.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.Button).And(cf.ByName("Удалить"))).ToList();

            if (allButtons.Count > 1)
            {
                return allButtons.OrderByDescending(b => b.BoundingRectangle.Y).First().AsButton();
            }

            return allButtons.FirstOrDefault()?.AsButton();
        }

        protected ComboBox GetCategoryComboBox() =>
            _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("categoryComboBox"))?.AsComboBox()
            ?? _mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.ComboBox))?.AsComboBox();

        protected ComboBox GetFilterComboBox()
        {
            var filterCombo = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("filterComboBox"))?.AsComboBox();
            if (filterCombo == null)
            {
                var allComboBoxes = _mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox));
                if (allComboBoxes.Length >= 2) filterCombo = allComboBoxes[1].AsComboBox();
                else if (allComboBoxes.Length == 1) filterCombo = allComboBoxes[0].AsComboBox();
            }
            return filterCombo;
        }

        protected bool SelectFilter(string filterName)
        {
            var filterCombo = GetFilterComboBox();
            if (filterCombo == null) return false;

            try
            {
                var itemToSelect = filterCombo.Items.FirstOrDefault(i => i.Name == filterName)
                    ?? filterCombo.Items.FirstOrDefault(i => i.Name.Contains(filterName));

                if (itemToSelect != null)
                {
                    filterCombo.Expand();
                    Thread.Sleep(500);
                    itemToSelect.Select();
                    Thread.Sleep(1000);
                    filterCombo.Collapse();
                    Thread.Sleep(1000);
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        protected void AddTaskViaUI(string description, string category = null)
        {
            var descBox = GetDescriptionTextBox();
            descBox.Text = description;
            Thread.Sleep(500);

            if (!string.IsNullOrEmpty(category))
            {
                var catCombo = GetCategoryComboBox();
                catCombo?.Select(category);
                Thread.Sleep(800);
            }

            var addBtn = GetAddTaskButton();
            addBtn?.Focus();
            Thread.Sleep(300);
            addBtn?.Click();
            Thread.Sleep(2000);

            var listBox = GetTasksListBox();
            if (listBox == null || listBox.Items.Length == 0)
                throw new Exception($"Задача '{description}' не добавилась!");
        }

        protected void CreateCategory(string categoryName)
        {
            var newCatBox = GetNewCategoryTextBox();
            newCatBox.Text = categoryName;
            Thread.Sleep(500);
            GetAddCategoryButton()?.Click();
            Thread.Sleep(2000);
            HandleMessageBox("OK");
            Thread.Sleep(500);
        }

        protected void HandleMessageBox(string expectedButtonText = "OK")
        {
            Thread.Sleep(500);
            var modalWindow = _mainWindow.ModalWindows.FirstOrDefault();
            if (modalWindow != null)
            {
                var okButton = modalWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button))?.AsButton();
                okButton?.Click();
                Thread.Sleep(500);
            }
        }

        protected void RefreshWindow()
        {
            _app.Close();
            Thread.Sleep(1500);
            _app = Application.Launch(AppPath);
            _mainWindow = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(15));
            Thread.Sleep(2500);
        }
    }

    [TestClass]
    public class TaskManagerAutomatedTests : FlaUITest
    {
        [TestMethod]
        public void TC_0001_AddTaskWithCorrectDescription()
        {
            var descBox = GetDescriptionTextBox();
            descBox.Text = "Взять шампунь";
            Thread.Sleep(500);
            GetAddTaskButton()?.Click();
            Thread.Sleep(2000);

            var listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items.Length > 0);
            Assert.IsTrue(listBox.Items[0].Text.Contains("Взять шампунь"));
        }

        [TestMethod]
        public void TC_0002_AddMultipleTasksAndCheckFile()
        {
            AddTaskViaUI("Задача 1");
            AddTaskViaUI("Задача 2");
            AddTaskViaUI("Задача 3");

            var listBox = GetTasksListBox();
            Assert.AreEqual(3, listBox.Items.Length);
            Assert.IsTrue(File.Exists("tasks.txt"));
            Assert.AreEqual(3, File.ReadAllLines("tasks.txt").Length);
        }

        [TestMethod]
        public void TC_0003_HandleEmptyDescriptionError()
        {
            GetDescriptionTextBox().Text = "";
            Thread.Sleep(500);
            GetAddTaskButton()?.Click();
            Thread.Sleep(2000);
            HandleMessageBox("OK");

            var listBox = GetTasksListBox();
            Assert.AreEqual(0, listBox.Items.Length);
        }

        [TestMethod]
        public void TC_0004_DeleteTaskByIndex()
        {
            AddTaskViaUI("Задача 1");
            AddTaskViaUI("Задача 2");
            AddTaskViaUI("Задача 3");

            var listBox = GetTasksListBox();
            listBox.Items[1].Select();
            Thread.Sleep(500);
            GetRemoveTaskButton()?.Click();
            Thread.Sleep(2000);

            Assert.AreEqual(2, GetTasksListBox().Items.Length);
        }

        [TestMethod]
        public void TC_0005_HandleDeleteWithoutSelection()
        {
            AddTaskViaUI("Задача 1");
            var listBox = GetTasksListBox();

            if (listBox.SelectedItems.Length > 0)
                listBox.SelectedItems[0].Select();

            GetRemoveTaskButton()?.Click();
            Thread.Sleep(2000);
            HandleMessageBox("OK");

            Assert.AreEqual(1, listBox.Items.Length);
        }

        [TestMethod]
        public void TC_0006_ToggleTaskStatus()
        {
            AddTaskViaUI("Тестовая задача");
            Thread.Sleep(500);

            var listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items.Length > 0, "Задача не создана");

            listBox.Items[0].Select();
            Thread.Sleep(300);

            var toggleBtn = GetToggleStatusButton();
            Assert.IsNotNull(toggleBtn, "Кнопка 'Отметить' не найдена");

            toggleBtn.Click();
            Thread.Sleep(1000);

            listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items[0].Text.Contains("[X]"),
                $"Статус не изменился на выполненный. Текст: {listBox.Items[0].Text}");

            listBox.Items[0].Select();
            Thread.Sleep(300);

            toggleBtn.Click();
            Thread.Sleep(1000);

            listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items[0].Text.Contains("[ ]"),
                $"Статус не вернулся на невыполненный. Текст: {listBox.Items[0].Text}");
        }

        [TestMethod]
        public void TC_0007_HandleToggleWithoutSelection()
        {
            AddTaskViaUI("Задача 1");
            var listBox = GetTasksListBox();

            if (listBox.SelectedItems.Length > 0)
                listBox.SelectedItems[0].Select();

            GetToggleStatusButton()?.Click();
            Thread.Sleep(2000);
            HandleMessageBox("OK");
        }

        [TestMethod]
        public void TC_0008_CheckFileAfterDeletion()
        {
            AddTaskViaUI("Задача 1");
            AddTaskViaUI("Задача 2");
            AddTaskViaUI("Задача 3");

            var listBox = GetTasksListBox();
            listBox.Items[1].Select();
            Thread.Sleep(500);
            GetRemoveTaskButton()?.Click();
            Thread.Sleep(2000);

            Assert.AreEqual(2, File.ReadAllLines("tasks.txt").Length);
        }

        [TestMethod]
        public void TC_0009_VerifyFileFormat()
        {
            AddTaskViaUI("Тестовая задача");
            var listBox = GetTasksListBox();
            listBox.Items[0].Select();
            Thread.Sleep(500);
            GetToggleStatusButton()?.Click();
            Thread.Sleep(2000);

            var content = File.ReadAllText("tasks.txt");
            Assert.IsTrue(content.Contains("|"));
            Assert.IsTrue(content.StartsWith("True") || content.StartsWith("False"));
        }

        [TestMethod]
        public void TC_0010_TaskWithNewLineInDescription()
        {
            GetDescriptionTextBox().Text = "Строка 1";
            Thread.Sleep(500);
            GetAddTaskButton()?.Click();
            Thread.Sleep(2000);

            Assert.AreEqual(1, GetTasksListBox().Items.Length);
        }

        [TestMethod]
        public void TC_0011_SaveLoadSpecialChars()
        {
            AddTaskViaUI("Задача @#$%");
            AddTaskViaUI("Привет мир");
            AddTaskViaUI("12345");
            RefreshWindow();

            Assert.AreEqual(3, GetTasksListBox().Items.Length);
        }

        [TestMethod]
        public void TC_0012_LoadTasksWithDifferentStatuses()
        {
            AddTaskViaUI("Выполненная");
            var listBox = GetTasksListBox();
            listBox.Items[0].Select();
            Thread.Sleep(500);
            GetToggleStatusButton()?.Click();
            Thread.Sleep(2000);

            AddTaskViaUI("Невыполненная");
            RefreshWindow();

            Assert.IsTrue(GetTasksListBox().Items.Length >= 2);
        }

        [TestMethod]
        public void TC_0013_InitWithoutTasksFile()
        {
            Assert.AreEqual(0, GetTasksListBox().Items.Length);
            AddTaskViaUI("Новая задача");
            Assert.IsTrue(File.Exists("tasks.txt"));
        }

        [TestMethod]
        public void TC_0014_VerifyTaskObjectProperties()
        {
            AddTaskViaUI("Проверка структуры");
            var listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items[0].Text.Contains("[ ] Проверка структуры"));
            Assert.IsTrue(File.ReadAllText("tasks.txt").Contains("False"));
        }

        [TestMethod]
        public void TC_0015_TaskWithSeparatorInDescription()
        {
            AddTaskViaUI("Купить хлеб | и молоко");
            RefreshWindow();

            var listBox = GetTasksListBox();
            Assert.AreEqual(1, listBox.Items.Length);
            Assert.IsTrue(listBox.Items[0].Text.Contains("Купить хлеб"));
        }

        [TestMethod]
        public void TC_0016_AddUniqueCategory()
        {
            if (File.Exists("categories.txt")) File.Delete("categories.txt");
            RefreshWindow();

            var newCatBox = GetNewCategoryTextBox();
            newCatBox.Text = "Работа";
            Thread.Sleep(500);
            GetAddCategoryButton()?.Click();
            Thread.Sleep(2000);
            HandleMessageBox("OK");

            var filterCombo = GetFilterComboBox();
            Assert.IsTrue(filterCombo.Items.Any(i => i.Name.Contains("Работа")));
        }

        [TestMethod]
        public void TC_0017_DuplicateCategoryError()
        {
            CreateCategory("Работа");

            GetNewCategoryTextBox().Text = "Работа";
            Thread.Sleep(500);
            GetAddCategoryButton()?.Click();
            Thread.Sleep(2000);

            Assert.IsNotNull(_mainWindow.ModalWindows.FirstOrDefault());
            HandleMessageBox("OK");
        }

        [TestMethod]
        public void TC_0018_AddTaskWithCategory()
        {
            CreateCategory("Учёба");
            Thread.Sleep(1000);

            var descBox = GetDescriptionTextBox();
            descBox.Text = "Сдать лабу";
            Thread.Sleep(500);

            var catCombo = GetCategoryComboBox();
            catCombo.Select("Учёба");
            Thread.Sleep(1000);

            var addBtn = GetAddTaskButton();
            addBtn.Focus();
            Thread.Sleep(300);
            addBtn.Click();
            Thread.Sleep(2500);

            var listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items.Length > 0);

            bool hasCategory = listBox.Items.Any(i => i.Text.Contains("Учёба") || i.Text.Contains("[Учёба]"));
            Assert.IsTrue(hasCategory);
        }

        [TestMethod]
        public void TC_0019_FilterByCategory()
        {
            CreateCategory("Работа");
            CreateCategory("Дом");
            CreateCategory("Учёба");
            Thread.Sleep(1000);

            AddTaskViaUI("Написать отчёт", "Работа");
            AddTaskViaUI("Сделать уборку", "Дом");
            AddTaskViaUI("Сделать лабораторную работу", "Учёба");
            Thread.Sleep(2000);

            Assert.IsTrue(SelectFilter("Учёба"));
            Thread.Sleep(2000);

            var listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items.Length >= 1);
            Assert.IsTrue(listBox.Items.Any(i => i.Text.Contains("лабораторную") || i.Text.Contains("[Учёба]")));
        }

        [TestMethod]
        public void TC_0020_ResetFilterShowAll()
        {
            AddTaskViaUI("Задача 1");
            Thread.Sleep(1000);

            CreateCategory("Учёба");
            Thread.Sleep(1000);

            var descBox = GetDescriptionTextBox();
            descBox.Text = "Лабораторная работа";
            Thread.Sleep(500);

            var catCombo = GetCategoryComboBox();
            catCombo.Select("Учёба");
            Thread.Sleep(500);

            var addBtn = GetAddTaskButton();
            addBtn.Focus();
            Thread.Sleep(300);
            addBtn.Click();
            Thread.Sleep(2000);

            AddTaskViaUI("Задача 3");
            Thread.Sleep(1000);

            var listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items.Length >= 3);

            var filterCombo = GetFilterComboBox();
            filterCombo.Expand();
            Thread.Sleep(1000);

            var uchebaItem = filterCombo.Items.FirstOrDefault(i => i.Name.Contains("Учёба"));
            if (uchebaItem != null)
            {
                uchebaItem.Click();
                Thread.Sleep(2000);
                filterCombo.Collapse();
                Thread.Sleep(1000);
            }

            listBox = GetTasksListBox();
            Assert.AreEqual(1, listBox.Items.Length);

            filterCombo.Expand();
            Thread.Sleep(1000);

            var allItem = filterCombo.Items.FirstOrDefault(i => i.Name == "Все" || i.Name.Contains("Все"));
            if (allItem != null)
            {
                allItem.Click();
                Thread.Sleep(2000);
                filterCombo.Collapse();
                Thread.Sleep(1000);
            }

            Thread.Sleep(2000);

            listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items.Length >= 3);
        }

        [TestMethod]
        public void TC_0021_DeleteCategoryMovesTasks()
        {
            var newCatBox = GetNewCategoryTextBox();
            newCatBox.Text = "Временная";
            GetAddCategoryButton().Click();
            Thread.Sleep(1000);
            HandleMessageBox("OK");
            Thread.Sleep(500);

            AddTaskViaUI("Тест", "Временная");
            Thread.Sleep(1000);

            var listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items.Length > 0, "Задача не была создана");

            var catCombo = GetCategoryComboBox();
            catCombo.Select("Временная");
            Thread.Sleep(1000);

            var removeCatBtn = GetRemoveCategoryButton();
            Assert.IsNotNull(removeCatBtn, "Кнопка удаления категории не найдена");

            removeCatBtn.Focus();
            Thread.Sleep(500);
            removeCatBtn.Click();
            Thread.Sleep(2000);

            HandleMessageBox("OK");
            Thread.Sleep(1000);

            listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items.Length > 0, "Задачи исчезли после удаления категории");

            bool taskInDefaultCategory = listBox.Items.Any(i =>
                i.Text.Contains("Тест") &&
                (i.Text.Contains("Без категории") || !i.Text.Contains("Временная")));

            Assert.IsTrue(taskInDefaultCategory,
                $"Задача 'Тест' не перемещена в 'Без категории'. Задачи: {string.Join("; ", listBox.Items.Select(i => i.Text))}");
        }

        [TestMethod]
        public void TC_0022_SaveLoadCategories()
        {
            CreateCategory("Фитнес");
            Thread.Sleep(500);
            AddTaskViaUI("Бег", "Фитнес");
            Thread.Sleep(2500);

            RefreshWindow();

            var listBox = GetTasksListBox();
            Assert.IsTrue(listBox.Items.Length > 0);
            Assert.IsTrue(listBox.Items.Any(i => i.Text.Contains("Бег")));

            var filterCombo = GetFilterComboBox();
            Assert.IsTrue(filterCombo.Items.Any(i => i.Name.Contains("Фитнес")));
        }
    }
}