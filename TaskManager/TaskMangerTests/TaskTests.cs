using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskManagerTests
{
    [TestClass]
    public class TaskTests
    {
        // Проверяет, что конструктор сохраняет описание задачи
        [TestMethod]
        public void Ctor_Desc()
        {
            string description = "Test task";
            var task = new TaskManager.Task(description);
            Assert.AreEqual(description, task.Description);
        }

        // Проверяет, что новая задача создаётся со статусом "не выполнена"
        [TestMethod]
        public void Ctor_Status()
        {
            var task = new TaskManager.Task("Test task");
            Assert.IsFalse(task.IsCompleted);
        }

        // Проверяет, что статус задачи можно изменить на "выполнена"
        [TestMethod]
        public void Status_ToTrue()
        {
            var task = new TaskManager.Task("Test task");
            task.IsCompleted = true;
            Assert.IsTrue(task.IsCompleted);
        }

        // Проверяет, что статус можно переключить обратно на "не выполнена"
        [TestMethod]
        public void Status_ToFalse()
        {
            var task = new TaskManager.Task("Test task");
            task.IsCompleted = true;
            task.IsCompleted = false;
            Assert.IsFalse(task.IsCompleted);
        }

        // Проверяет, что описание задачи можно изменить после создания
        [TestMethod]
        public void Desc_Change()
        {
            var task = new TaskManager.Task("Original task");
            task.Description = "Modified task";
            Assert.AreEqual("Modified task", task.Description);
        }

        // Проверяет, что задача может содержать специальные символы
        [TestMethod]
        public void Desc_SpecialChars()
        {
            string description = "Task with !@#$%^&*()";
            var task = new TaskManager.Task(description);
            Assert.AreEqual(description, task.Description);
        }

        // Проверяет, что задача может содержать Unicode-символы (эмодзи, кириллицу)
        [TestMethod]
        public void Desc_Unicode()
        {
            string description = "Задача с эмодзи 😉";
            var task = new TaskManager.Task(description);
            Assert.AreEqual(description, task.Description);
        }

        // Проверяет, что задача может содержать очень длинное описание
        [TestMethod]
        public void Desc_Long()
        {
            string longDescription = new string('A', 10000);
            var task = new TaskManager.Task(longDescription);
            Assert.AreEqual(longDescription, task.Description);
        }

        // Проверяет, что разные экземпляры задачи не влияют друг на друга
        [TestMethod]
        public void Instances_Independent()
        {
            var task1 = new TaskManager.Task("Task 1");
            var task2 = new TaskManager.Task("Task 2");

            task1.IsCompleted = true;
            task1.Description = "Modified Task 1";

            Assert.IsTrue(task1.IsCompleted);
            Assert.IsFalse(task2.IsCompleted);
            Assert.AreEqual("Modified Task 1", task1.Description);
            Assert.AreEqual("Task 2", task2.Description);
        }

        // Проверяет, что описание задачи можно менять несколько раз подряд
        [TestMethod]
        public void Desc_MultiChange()
        {
            var task = new TaskManager.Task("First");
            task.Description = "Second";
            task.Description = "Third";
            task.Description = "Final";
            Assert.AreEqual("Final", task.Description);
        }
    }
}