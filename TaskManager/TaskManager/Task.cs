using System;

namespace TaskManager
{
    public class Task
    {
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public string Category { get; set; }

        public Task(string description, string category = "Без категории")
        {
            Description = description;
            IsCompleted = false;
            Category = string.IsNullOrWhiteSpace(category) ? "Без категории" : category;
        }

        public override string ToString()
        {
            return $"[{Category}] {(IsCompleted ? "[X]" : "[ ]")} {Description}";
        }
    }
}