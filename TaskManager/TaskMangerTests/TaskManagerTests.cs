using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace TaskManagerTests
{
    [TestClass]
    public class TaskManagerTests
    {
        private TaskManager.TaskManager taskManager;
        private string testFilePath = "tasks.txt";

        [TestInitialize]
        public void Setup()
        {
            if (File.Exists(testFilePath)) File.Delete(testFilePath);
            taskManager = new TaskManager.TaskManager();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(testFilePath)) File.Delete(testFilePath);
        }

        // Проверяет, что конструктор создаёт пустой список при отсутствии файла
        [TestMethod]
        public void NewManager_IsEmpty()
        {
            Assert.AreEqual(0, taskManager.Tasks.Count);
        }

        // Проверяет, что AddTask сохраняет изменения в файл
        [TestMethod]
        public void Add_SavesToFile()
        {
            taskManager.AddTask("Task to save");
            Assert.IsTrue(File.Exists(testFilePath));
            var content = File.ReadAllText(testFilePath);
            Assert.IsTrue(content.Contains("Task to save"));
        }

        // Проверяет, что RemoveTask обновляет файл после удаления
        [TestMethod]
        public void Remove_UpdatesFile()
        {
            taskManager.AddTask("Task 1");
            taskManager.AddTask("Task 2");
            taskManager.RemoveTask(0);
            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(1, lines.Length);
            Assert.IsTrue(lines[0].Contains("Task 2"));
        }

        // Проверяет, что ToggleTaskCompletion обновляет файл
        [TestMethod]
        public void Toggle_UpdatesFile()
        {
            taskManager.AddTask("Task 1");
            taskManager.ToggleTaskCompletion(0);
            var content = File.ReadAllText(testFilePath);
            Assert.IsTrue(content.StartsWith("True|"));
        }

        // Проверяет, что LoadTasks пропускает некорректные строки в файле
        [TestMethod]
        public void Load_SkipsBadLines()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "True|Valid Task",
                "InvalidLine",
                "False|Another Valid",
                "Maybe|Task|Extra"
            });
            var newManager = new TaskManager.TaskManager();
            Assert.AreEqual(2, newManager.Tasks.Count);
            Assert.AreEqual("Valid Task", newManager.Tasks[0].Description);
            Assert.AreEqual("Another Valid", newManager.Tasks[1].Description);
        }

        // Проверяет, что LoadTasks корректно обрабатывает пустой файл
        [TestMethod]
        public void Load_EmptyFile_DoesNotCrash()
        {
            File.WriteAllText(testFilePath, "");
            var newManager = new TaskManager.TaskManager();
            Assert.AreEqual(0, newManager.Tasks.Count);
        }

        // Проверяет, что свойство Tasks имеет огранниченный доступ на запись
        [TestMethod]
        public void Tasks_CannotBeReassigned()
        {
            var property = typeof(TaskManager.TaskManager).GetProperty("Tasks");
            Assert.IsNotNull(property);
            Assert.IsTrue(property.GetSetMethod(true).IsPrivate);
        }

        // Проверяет, что после удаления задачи, индексы сдвигаются корректно
        [TestMethod]
        public void Remove_Middle_ShiftsIndices()
        {
            taskManager.AddTask("Task 1");
            taskManager.AddTask("Task 2");
            taskManager.AddTask("Task 3");
            taskManager.RemoveTask(1);
            Assert.AreEqual(2, taskManager.Tasks.Count);
            Assert.AreEqual("Task 1", taskManager.Tasks[0].Description);
            Assert.AreEqual("Task 3", taskManager.Tasks[1].Description);
        }

        // Проверяет, что ToggleTaskCompletion работает с несколькими задачами
        [TestMethod]
        public void Toggle_Multiple_WorksIndependently()
        {
            taskManager.AddTask("Task 1");
            taskManager.AddTask("Task 2");
            taskManager.AddTask("Task 3");
            taskManager.ToggleTaskCompletion(0);
            taskManager.ToggleTaskCompletion(2);
            Assert.IsTrue(taskManager.Tasks[0].IsCompleted);
            Assert.IsFalse(taskManager.Tasks[1].IsCompleted);
            Assert.IsTrue(taskManager.Tasks[2].IsCompleted);
        }

        // Проверяет формат сохранения: "IsCompleted|Description"
        [TestMethod]
        public void Save_Format_IsCorrect()
        {
            taskManager.AddTask("Test Task");
            taskManager.ToggleTaskCompletion(0);
            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(1, lines.Length);
            Assert.AreEqual("True|Test Task", lines[0]);
        }

        // Проверяет, что специальные символы в описании корректно сохраняются
        [TestMethod]
        public void Save_SpecialChars_Preserved()
        {
            string desc = "Task with | pipe and special chars !@#";
            taskManager.AddTask(desc);
            var content = File.ReadAllText(testFilePath);
            Assert.IsTrue(content.Contains(desc));
        }

        // Проверяет, что список Tasks нельзя изменить извне
        [TestMethod]
        public void Tasks_SameReference_AfterAdd()
        {
            var originalList = taskManager.Tasks;
            taskManager.AddTask("Test");
            Assert.AreSame(originalList, taskManager.Tasks);
        }
    }
}