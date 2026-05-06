using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TaskManager
{
    public class TaskManager
    {
        public List<Task> Tasks { get; private set; }

        public TaskManager()
        {
            Tasks = new List<Task>();
            LoadTasks();
        }

        public void AddTask(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException("Описание задачи не может быть пустым.");
            }
            Tasks.Add(new Task(description));
            SaveTasks();
        }

        public void RemoveTask(int index)
        {
            if (index < 0 || index >= Tasks.Count)
            {
                throw new IndexOutOfRangeException("Некорректный индекс задачи.");
            }
            Tasks.RemoveAt(index);
            SaveTasks();
        }

        public void ToggleTaskCompletion(int index)
        {
            if (index < 0 || index >= Tasks.Count)
            {
                throw new IndexOutOfRangeException("Некорректный индекс задачи.");
            }
            Tasks[index].IsCompleted = !Tasks[index].IsCompleted;
            SaveTasks();
        }

        private void SaveTasks()
        {
            File.WriteAllLines("tasks.txt", Tasks.Select(t => $"{t.IsCompleted}|{t.Description}"));
        }
        private void LoadTasks()
        {
            if (File.Exists("tasks.txt"))
            {
                var lines = File.ReadAllLines("tasks.txt");
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { '|' }, 2);// исправленная строчка
                    if (parts.Length == 2)
                    {
                        bool isCompleted = bool.Parse(parts[0]);
                        string description = parts[1];
                        Tasks.Add(new Task(description) { IsCompleted = isCompleted });
                    }
                }
            }
        }
    }
}