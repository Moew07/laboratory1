using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TaskManager
{
    public class TaskManager
    {
        public List<Task> Tasks { get; private set; }
        public List<string> Categories { get; private set; }
        private readonly string _tasksFile;
        private readonly string _categoriesFile;
        public TaskManager(string tasksFile = "tasks.txt", string categoriesFile = "categories.txt")
        {
            Tasks = new List<Task>();
            Categories = new List<string> { "Без категории" };
            _tasksFile = tasksFile;
            _categoriesFile = categoriesFile;

            LoadCategories();
            LoadTasks();
        }

        //категории 
        public bool AddCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            name = name.Trim();
            if (Categories.Contains(name, StringComparer.OrdinalIgnoreCase)) return false;
            Categories.Add(name);
            SaveCategories();
            return true;
        }

        public bool RemoveCategory(string name)
        {
            if (name == "Без категории" || !Categories.Contains(name)) return false;
            Categories.Remove(name);
            foreach (var task in Tasks.Where(t => t.Category == name))
            {
                task.Category = "Без категории";
            }
            SaveCategories();
            SaveTasks();
            return true;
        }

        public List<string> GetCategories() => new List<string>(Categories);

        //задачи 
        public void AddTask(string description, string category = "Без категории")
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Описание задачи не может быть пустым.");

            if (!Categories.Contains(category)) category = "Без категории";
            Tasks.Add(new Task(description, category));
            SaveTasks();
        }

        public void RemoveTask(int index)
        {
            if (index < 0 || index >= Tasks.Count)
                throw new IndexOutOfRangeException("Некорректный индекс задачи.");
            Tasks.RemoveAt(index);
            SaveTasks();
        }

        public void ToggleTaskCompletion(int index)
        {
            if (index < 0 || index >= Tasks.Count)
                throw new IndexOutOfRangeException("Некорректный индекс задачи.");
            Tasks[index].IsCompleted = !Tasks[index].IsCompleted;
            SaveTasks();
        }

        public void UpdateTaskCategory(int index, string newCategory)
        {
            if (index < 0 || index >= Tasks.Count)
                throw new IndexOutOfRangeException("Некорректный индекс задачи.");
            if (!Categories.Contains(newCategory)) newCategory = "Без категории";
            Tasks[index].Category = newCategory;
            SaveTasks();
        }

        //  фильтрация 
        public List<Task> GetTasksByCategory(string category)
        {
            return string.IsNullOrEmpty(category) || category == "Все"
                ? new List<Task>(Tasks)
                : Tasks.Where(t => t.Category == category).ToList();
        }

        // сохранение/загрузка 
        private void SaveTasks()
        {
            var lines = Tasks.Select(t => $"{t.IsCompleted}|{t.Category}|{t.Description}");
            File.WriteAllLines(_tasksFile, lines);
        }

        private void LoadTasks()
        {
            if (!File.Exists(_tasksFile)) return;
            var lines = File.ReadAllLines(_tasksFile);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(new[] { '|' }, 3);
                if (parts.Length >= 3)
                {
                    bool isCompleted = bool.Parse(parts[0]);
                    string category = parts[1];
                    string description = parts[2];
                    if (!Categories.Contains(category)) Categories.Add(category);
                    Tasks.Add(new Task(description, category) { IsCompleted = isCompleted });
                }
            }
        }

        private void SaveCategories()
        {
            File.WriteAllLines(_categoriesFile, Categories);
        }

        private void LoadCategories()
        {
            if (!File.Exists(_categoriesFile)) return;
            var lines = File.ReadAllLines(_categoriesFile);
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && !Categories.Contains(line.Trim()))
                    Categories.Add(line.Trim());
            }
            if (!Categories.Contains("Без категории"))
                Categories.Add("Без категории");
        }
    }
}