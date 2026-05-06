using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace TaskManagerTests
{
    [TestClass]
    public class CategoryTests
    {
        private TaskManager.TaskManager taskManager;
        private readonly string tasksFile = "test_tasks.txt";
        private readonly string categoriesFile = "test_categories.txt";

        [TestInitialize]
        public void Setup()
        {
            // Очистка тестовых файлов перед каждым запуском
            if (File.Exists(tasksFile)) File.Delete(tasksFile);
            if (File.Exists(categoriesFile)) File.Delete(categoriesFile);
            taskManager = new TaskManager.TaskManager(tasksFile, categoriesFile);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Удаление тестовых файлов после каждого теста
            if (File.Exists(tasksFile)) File.Delete(tasksFile);
            if (File.Exists(categoriesFile)) File.Delete(categoriesFile);
        }

        // проверяет, что новая категория успешно добавляется в список
        [TestMethod]
        public void AddCategory_Valid_Adds()
        {
            bool result = taskManager.AddCategory("Работа");
            Assert.IsTrue(result);
            Assert.IsTrue(taskManager.GetCategories().Contains("Работа"));
        }

        // проверяет, что категория с уже существующим именем не добавляется
        [TestMethod]
        public void AddCategory_Duplicate_Fails()
        {
            taskManager.AddCategory("Дом");
            bool result = taskManager.AddCategory("Дом");
            Assert.IsFalse(result);
        }

        // проверяет, что пустое имя или имя из пробелов отклоняется
        [TestMethod]
        public void AddCategory_EmptyName_Fails()
        {
            Assert.IsFalse(taskManager.AddCategory(""));
            Assert.IsFalse(taskManager.AddCategory("   "));
        }

        // проверяет, что удаление категории переносит её задачи в "Без категории"
        [TestMethod]
        public void RemoveCategory_Existing_MovesTasks()
        {
            taskManager.AddCategory("Temp");
            taskManager.AddTask("Задача 1", "Temp");
            taskManager.RemoveCategory("Temp");
            Assert.IsFalse(taskManager.GetCategories().Contains("Temp"));
            Assert.AreEqual("Без категории", taskManager.Tasks[0].Category);
        }

        // проверяет, что системную категорию "Без категории" нельзя удалить
        [TestMethod]
        public void RemoveCategory_Default_Prevented()
        {
            bool result = taskManager.RemoveCategory("Без категории");
            Assert.IsFalse(result);
        }
        // проверяет, что задаётся корректная выбранная категория
        [TestMethod]
        public void AddTask_WithCategory_Assigned()
        {
            taskManager.AddCategory("Учёба");
            taskManager.AddTask("Сделать ДЗ", "Учёба");
            Assert.AreEqual("Учёба", taskManager.Tasks[0].Category);
        }

        // проверяет, что отсутствие или неверная категория заменяется на дефолтную
        [TestMethod]
        public void AddTask_NoOrInvalidCategory_UsesDefault()
        {
            taskManager.AddTask("Просто задача");
            taskManager.AddTask("Задача 2", "Несуществующая");
            Assert.AreEqual("Без категории", taskManager.Tasks[0].Category);
            Assert.AreEqual("Без категории", taskManager.Tasks[1].Category);
        }

        // проверяет, что категорию существующей задачи можно изменить
        [TestMethod]
        public void UpdateTaskCategory_Existing_Changes()
        {
            taskManager.AddCategory("Работа");
            taskManager.AddTask("Отчёт", "Без категории");
            taskManager.UpdateTaskCategory(0, "Работа");
            Assert.AreEqual("Работа", taskManager.Tasks[0].Category);
        }

        // проверяет, что фильтр возвращает только задачи выбранной категории
        [TestMethod]
        public void FilterByCategory_ReturnsMatched()
        {
            taskManager.AddCategory("Дом");
            taskManager.AddTask("Хлеб", "Дом");
            taskManager.AddTask("Отчёт", "Работа");
            var filtered = taskManager.GetTasksByCategory("Дом");
            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual("Хлеб", filtered[0].Description);
        }

        // проверяет, что фильтр "Все" или пустая строка возвращает полный список
        [TestMethod]
        public void FilterAllOrEmpty_ReturnsAll()
        {
            taskManager.AddTask("Задача 1", "Работа");
            taskManager.AddTask("Задача 2");
            Assert.AreEqual(2, taskManager.GetTasksByCategory("Все").Count);
            Assert.AreEqual(2, taskManager.GetTasksByCategory("").Count);
        }

        // проверяет, что список категорий корректно записывается в файл
        [TestMethod]
        public void SaveCategories_WritesToFile()
        {
            taskManager.AddCategory("TestCat");
            Assert.IsTrue(File.Exists(categoriesFile));
            Assert.IsTrue(File.ReadAllLines(categoriesFile).Contains("TestCat"));
        }

        // проверяет, что категории успешно загружаются из файла при создании менеджера
        [TestMethod]
        public void LoadCategories_ReadsFromFile()
        {
            File.WriteAllLines(categoriesFile, new[] { "Work", "Home" });
            var newManager = new TaskManager.TaskManager(tasksFile, categoriesFile);
            Assert.IsTrue(newManager.GetCategories().Contains("Work"));
        }

        // проверяет, что задачи с их категориями и статусами сохраняются и восстанавливаются
        [TestMethod]
        public void SaveLoadTasks_PersistsData()
        {
            taskManager.AddCategory("Cat1");
            taskManager.AddTask("Task1", "Cat1");
            taskManager.ToggleTaskCompletion(0);

            var newManager = new TaskManager.TaskManager(tasksFile, categoriesFile);
            Assert.AreEqual(1, newManager.Tasks.Count);
            Assert.AreEqual("Task1", newManager.Tasks[0].Description);
            Assert.AreEqual("Cat1", newManager.Tasks[0].Category);
            Assert.IsTrue(newManager.Tasks[0].IsCompleted);
        }

        // проверяет, что категории с символами и Unicode сохраняются без ошибок парсинга
        [TestMethod]
        public void Category_SpecialChars_Preserved()
        {
            string name = "Категория | с !@# и эмодзи 📁";
            taskManager.AddCategory(name);
            taskManager.AddTask("Тест", name);
            Assert.AreEqual(name, taskManager.Tasks[0].Category);
        }
    }
}