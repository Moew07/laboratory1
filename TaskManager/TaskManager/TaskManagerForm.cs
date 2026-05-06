using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace TaskManager
{
    public partial class TaskManagerForm : Form
    {
        private TaskManager taskManager;
        private ListBox tasksListBox;
        private TextBox descriptionTextBox;
        private TextBox indexTextBox;
        private ComboBox categoryComboBox;
        private ComboBox filterComboBox;
        private TextBox newCategoryTextBox;
        private Button addTaskButton, removeTaskButton, toggleButton;
        private Button addCategoryButton, removeCategoryButton;
        private Button filterButton;
        private Label descLabel, indexLabel, catLabel, filterLabel, newCatLabel;

        private string currentFilter = "Все";

        public TaskManagerForm()
        {
            this.Text = "Управление задачами с категориями";
            this.Width = 500;
            this.Height = 550;
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeControls();
            taskManager = new TaskManager();
            PopulateCategories();
            UpdateTasksList();
        }

        private void InitializeControls()
        {
            //левая панель(список задач)
            tasksListBox = new ListBox { Location = new Point(10, 10), Width = 220, Height = 300 };
            tasksListBox.SelectedIndexChanged += TasksListBox_SelectedIndexChanged;

            //правая панель(управление задачами)
            descLabel = new Label { Text = "Описание:", Location = new Point(240, 10), AutoSize = true };
            descriptionTextBox = new TextBox { Location = new Point(240, 30), Width = 230 };

            catLabel = new Label { Text = "Категория:", Location = new Point(240, 58), AutoSize = true };
            categoryComboBox = new ComboBox { Location = new Point(240, 78), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };

            indexLabel = new Label { Text = "Индекс:", Location = new Point(240, 106), AutoSize = true };
            indexTextBox = new TextBox { Location = new Point(240, 126), Width = 50 };

            addTaskButton = new Button { Text = "Добавить задачу", Location = new Point(240, 155), Width = 110 };
            addTaskButton.Click += AddTaskButton_Click;

            removeTaskButton = new Button { Text = "Удалить", Location = new Point(360, 155), Width = 110 };
            removeTaskButton.Click += RemoveTaskButton_Click;

            toggleButton = new Button { Text = "Отметить выполнение", Location = new Point(240, 185), Width = 230 };
            toggleButton.Click += ToggleCompletionButton_Click;

            //управление категориями
            newCatLabel = new Label { Text = "Новая категория:", Location = new Point(240, 220), AutoSize = true };
            newCategoryTextBox = new TextBox { Location = new Point(240, 240), Width = 150 };

            addCategoryButton = new Button { Text = "Добавить", Location = new Point(240, 265), Width = 70 };
            addCategoryButton.Click += AddCategoryButton_Click;

            removeCategoryButton = new Button { Text = "Удалить", Location = new Point(320, 265), Width = 70 };
            removeCategoryButton.Click += RemoveCategoryButton_Click;

            // фильтрация
            filterLabel = new Label { Text = "Фильтр:", Location = new Point(240, 300), AutoSize = true };
            filterComboBox = new ComboBox { Location = new Point(240, 320), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            filterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;

            //добавляем элементы на форму
            this.Controls.AddRange(new Control[] {
                tasksListBox, descLabel, descriptionTextBox, catLabel, categoryComboBox,
                indexLabel, indexTextBox, addTaskButton, removeTaskButton, toggleButton,
                newCatLabel, newCategoryTextBox, addCategoryButton, removeCategoryButton,
                filterLabel, filterComboBox
            });
        }

        private void PopulateCategories()
        {
            categoryComboBox.Items.Clear();
            filterComboBox.Items.Clear();
            filterComboBox.Items.Add("Все");

            foreach (var cat in taskManager.GetCategories())
            {
                categoryComboBox.Items.Add(cat);
                filterComboBox.Items.Add(cat);
            }

            categoryComboBox.SelectedIndex = 0;
            filterComboBox.SelectedIndex = 0;
        }

        private void UpdateTasksList()
        {
            tasksListBox.Items.Clear();
            var tasks = taskManager.GetTasksByCategory(currentFilter);
            for (int i = 0; i < tasks.Count; i++)
            {
                tasksListBox.Items.Add($"{i}. {tasks[i]}");
            }
        }

        private void TasksListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tasksListBox.SelectedIndex >= 0)
            {
                var tasks = taskManager.GetTasksByCategory(currentFilter);
                var task = tasks[tasksListBox.SelectedIndex];
                descriptionTextBox.Text = task.Description;
                categoryComboBox.SelectedItem = task.Category;
            }
        }

        private void AddTaskButton_Click(object sender, EventArgs e)
        {
            try
            {
                string cat = categoryComboBox.SelectedItem?.ToString() ?? "Без категории";
                taskManager.AddTask(descriptionTextBox.Text, cat);
                descriptionTextBox.Clear();
                PopulateCategories();
                UpdateTasksList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RemoveTaskButton_Click(object sender, EventArgs e)
        {
            if (tasksListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите задачу для удаления!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                var tasks = taskManager.GetTasksByCategory(currentFilter);
                var realIndex = taskManager.Tasks.IndexOf(tasks[tasksListBox.SelectedIndex]);
                taskManager.RemoveTask(realIndex);
                PopulateCategories();
                UpdateTasksList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ToggleCompletionButton_Click(object sender, EventArgs e)
        {
            if (tasksListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите задачу!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                var tasks = taskManager.GetTasksByCategory(currentFilter);
                var realIndex = taskManager.Tasks.IndexOf(tasks[tasksListBox.SelectedIndex]);
                taskManager.ToggleTaskCompletion(realIndex);
                UpdateTasksList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddCategoryButton_Click(object sender, EventArgs e)
        {
            string name = newCategoryTextBox.Text.Trim();
            if (taskManager.AddCategory(name))
            {
                newCategoryTextBox.Clear();
                PopulateCategories();
                MessageBox.Show($"Категория «{name}» добавлена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Категория с таким именем уже существует!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveCategoryButton_Click(object sender, EventArgs e)
        {
            string name = categoryComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(name) || name == "Без категории")
            {
                MessageBox.Show("Выберите категорию для удаления (кроме «Без категории»)", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (taskManager.RemoveCategory(name))
            {
                PopulateCategories();
                UpdateTasksList();
                MessageBox.Show($"Категория «{name}» удалена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentFilter = filterComboBox.SelectedItem?.ToString() ?? "Все";
            UpdateTasksList();
        }
    }
}