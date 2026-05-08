using System;

namespace CybersecurityChatbot.Models
{
    // This class represents a task created by the user.
    // It is used in the Task Assistant feature of the chatbot.
    public class TaskItem
    {
        // Stores the unique ID of the task.
        // This is usually used to identify the task in the database.
        public int Id { get; set; }

        // Stores the title (name) of the task.
        // Example: "Review privacy settings"
        public string Title { get; set; } = string.Empty;

        // Stores a detailed description of the task.
        // Example: "Check all social media privacy options"
        public string Description { get; set; } = string.Empty;

        // Stores the reminder date for the task (if the user sets one).
        // The '?' means this value can be null (no reminder set).
        public DateTime? ReminderDate { get; set; }

        // Indicates whether the task has been completed or not.
        // True = completed, False = not completed
        public bool IsCompleted { get; set; }
    }
}