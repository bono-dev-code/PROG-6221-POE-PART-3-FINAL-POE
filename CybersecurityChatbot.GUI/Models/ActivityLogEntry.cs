using System;

namespace CybersecurityChatbot.Models
{
    // This class represents a single activity log entry in the system.
    // It stores information about what happened, when it happened, and a short description.
    public class ActivityLogEntry
    {
        // Stores the date and time when the action happened.
        // It automatically sets the current date and time when a new log is created.
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Stores the type of action (e.g., "Add Task", "Delete Task", "Quiz Completed").
        public string ActionType { get; set; } = string.Empty;

        // Stores a detailed description of what happened.
        public string Description { get; set; } = string.Empty;

        // This method converts the log entry into a readable string format.
        // It will be used when displaying logs to the user.
        public override string ToString()
        {
            // Example output:
            // [2026-04-30 21:10] Add Task: User added a new task
            return $"[{Timestamp:yyyy-MM-dd HH:mm}] {ActionType}: {Description}";
        }
    }
}