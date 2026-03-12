using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager
{
    public class Task
    {
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public Task(string description)
        {
            Description = description;
            IsCompleted = false;
        }
    }

}
