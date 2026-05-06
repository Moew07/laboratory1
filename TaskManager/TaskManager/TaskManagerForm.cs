using System;
using System.Drawing;
using System.Windows.Forms;

namespace TaskManager
{
    public partial class TaskManagerForm : Form
    {
        private TaskManager taskManager;
        private ListBox tasksListBox;
        private TextBox descriptionTextBox;
        private TextBox indexTextBox;
        private Button addTaskButton;
        private Button removeTaskButton;
        private Button toggleCompletionButton;
        private Label descriptionLabel;
        private Label indexLabel;

        public TaskManagerForm()
        {
            this.Text = "Управление задачами";
            this.Width = 400;
            this.Height = 400; 
            this.StartPosition = FormStartPosition.CenterScreen;

            // Label "Описание:"
            descriptionLabel = new Label
            {
                Text = "Описание:",
                Location = new Point(220, 10),
                AutoSize = true
            };

            descriptionTextBox = new TextBox
            {
                Location = new Point(220, 30), 
                Width = 150, 
            };

            //  Label "Индекс:"
            indexLabel = new Label
            {
                Text = "Индекс:",
                Location = new Point(220, 58),
                AutoSize = true
            };

            //  Поле ввода индекса
            indexTextBox = new TextBox
            {
                Location = new Point(220, 78),
                Width = 50
            };

            addTaskButton = new Button
            {
                Location = new Point(220, 105), 
                Text = "Добавить",
                Width = 70
            };
            addTaskButton.Click += AddTaskButton_Click;

            removeTaskButton = new Button
            {
                Location = new Point(300, 105), 
                Text = "Удалить",
                Width = 70
            };
            removeTaskButton.Click += RemoveTaskButton_Click;

            toggleCompletionButton = new Button
            {
                Location = new Point(220, 135), 
                Text = "Отметить",
                Width = 150
            };
            toggleCompletionButton.Click += ToggleCompletionButton_Click;

            tasksListBox = new ListBox
            {
                Location = new Point(10, 10),
                Width = 200,
                Height = 200 
            };


            this.Controls.Add(tasksListBox);
            this.Controls.Add(descriptionLabel);
            this.Controls.Add(descriptionTextBox);
            this.Controls.Add(indexLabel);
            this.Controls.Add(indexTextBox);
            this.Controls.Add(addTaskButton);
            this.Controls.Add(removeTaskButton);
            this.Controls.Add(toggleCompletionButton);

            taskManager = new TaskManager();
            UpdateTasksList();
        }

        private void UpdateTasksList()
        {
            tasksListBox.Items.Clear();
            for (int i = 0; i < taskManager.Tasks.Count; i++)
            {
                var task = taskManager.Tasks[i];
                string status = task.IsCompleted ? "[X]" : "[ ]";
                tasksListBox.Items.Add($"{i}. {status} {task.Description}");
            }
        }

        private void AddTaskButton_Click(object sender, EventArgs e)
        {
            try
            {
                taskManager.AddTask(descriptionTextBox.Text);
                descriptionTextBox.Clear();
                UpdateTasksList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RemoveTaskButton_Click(object sender, EventArgs e)
        {
            int index = -1;

        
            if (!string.IsNullOrWhiteSpace(indexTextBox.Text))
            {
                if (!int.TryParse(indexTextBox.Text, out index))
                {
                    MessageBox.Show("Введите корректный числовой индекс!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
       
            else if (tasksListBox.SelectedIndex != -1)
            {
                index = tasksListBox.SelectedIndex;
            }
          
            else
            {
                MessageBox.Show("Введите индекс задачи или выберите задачу в списке для удаления!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                taskManager.RemoveTask(index);
                indexTextBox.Clear();
                UpdateTasksList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ToggleCompletionButton_Click(object sender, EventArgs e)
        {
            if (tasksListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите задачу для изменения статуса!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                taskManager.ToggleTaskCompletion(tasksListBox.SelectedIndex);
                UpdateTasksList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}