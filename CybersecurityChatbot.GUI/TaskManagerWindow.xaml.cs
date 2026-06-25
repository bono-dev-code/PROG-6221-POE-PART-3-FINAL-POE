using System;
using System.Windows;
using CybersecurityChatbot.Models;
using CybersecurityChatbot.Services;

namespace CybersecurityChatbot.GUI
{
    public partial class TaskManagerWindow : Window
    {
        private readonly DatabaseService _databaseService;
        private readonly ActivityLogService _activityLogService;

        public TaskManagerWindow(DatabaseService databaseService, ActivityLogService activityLogService)
        {
            InitializeComponent();

            _databaseService = databaseService;
            _activityLogService = activityLogService;

            LoadTasks();
        }

        private void LoadTasks()
        {
            try
            {
                TaskDataGrid.ItemsSource = null;
                TaskDataGrid.ItemsSource = _databaseService.GetTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load tasks from the database.\n" + ex.Message);
            }
        }

        private bool TryGetSelectedTask(out TaskItem? selectedTask)
        {
            selectedTask = TaskDataGrid.SelectedItem as TaskItem;
            return selectedTask != null;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
            _activityLogService.Add("Task", "Task list refreshed.");
        }

        private void MarkComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetSelectedTask(out TaskItem? selectedTask) || selectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            _databaseService.MarkTaskCompleted(selectedTask.Id);
            _activityLogService.Add("Task", $"Task #{selectedTask.Id} marked as completed.");
            LoadTasks();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetSelectedTask(out TaskItem? selectedTask) || selectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete '{selectedTask.Title}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _databaseService.DeleteTask(selectedTask.Id);
                _activityLogService.Add("Task", $"Task #{selectedTask.Id} deleted.");
                LoadTasks();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}