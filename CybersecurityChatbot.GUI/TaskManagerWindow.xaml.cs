using System;
using System.Windows;
using CybersecurityChatbot.Models;
using CybersecurityChatbot.Services;

namespace CybersecurityChatbot.GUI
{
    // Window that displays and manages tasks in a data grid
    public partial class TaskManagerWindow : Window
    {
        // Services this window needs to work
        private readonly DatabaseService _databaseService;      // Saves/loads tasks from database
        private readonly ActivityLogService _activityLogService; // Tracks user actions

        // Constructor - runs when the Task Manager window opens
        public TaskManagerWindow(DatabaseService databaseService, ActivityLogService activityLogService)
        {
            InitializeComponent(); // Sets up all the UI elements

            // Store references to services passed from MainWindow
            _databaseService = databaseService;
            _activityLogService = activityLogService;

            LoadTasks(); // Load tasks from database and display them
        }

        // Loads all tasks from the database and shows them in the data grid
        private void LoadTasks()
        {
            try
            {
                // Clear existing data and load fresh from database
                TaskDataGrid.ItemsSource = null;  // Reset the grid
                TaskDataGrid.ItemsSource = _databaseService.GetTasks(); // Load tasks
            }
            catch (Exception ex)
            {
                // Show error message if something goes wrong
                MessageBox.Show("Could not load tasks from the database.\n" + ex.Message);
            }
        }

        // Helper method to get the task currently selected in the grid
        private bool TryGetSelectedTask(out TaskItem? selectedTask)
        {
            // Try to cast the selected item to a TaskItem
            selectedTask = TaskDataGrid.SelectedItem as TaskItem;
            return selectedTask != null; // Returns true if we got a valid task
        }

        // Handles Refresh button click - reloads tasks from database
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks(); // Reload tasks
            _activityLogService.Add("Task", "Task list refreshed."); // Log the action
        }

        // Handles Mark Complete button click - marks selected task as done
        private void MarkComplete_Click(object sender, RoutedEventArgs e)
        {
            // Check if a task is selected
            if (!TryGetSelectedTask(out TaskItem? selectedTask) || selectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            // Mark the task as completed in database
            _databaseService.MarkTaskCompleted(selectedTask.Id);
            _activityLogService.Add("Task", $"Task #{selectedTask.Id} marked as completed.");
            LoadTasks(); // Refresh the grid to show updated status
        }

        // Handles Delete button click - deletes selected task
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // Check if a task is selected
            if (!TryGetSelectedTask(out TaskItem? selectedTask) || selectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            // Ask for confirmation before deleting
            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete '{selectedTask.Title}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,      // Shows Yes/No buttons
                MessageBoxImage.Warning);    // Shows warning icon

            // If user clicked Yes, delete the task
            if (result == MessageBoxResult.Yes)
            {
                _databaseService.DeleteTask(selectedTask.Id);
                _activityLogService.Add("Task", $"Task #{selectedTask.Id} deleted.");
                LoadTasks(); // Refresh the grid
            }
            // If user clicked No, do nothing
        }

        // Handles Close button click - closes the Task Manager window
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close(); // Built-in method to close the window
        }
    }
}