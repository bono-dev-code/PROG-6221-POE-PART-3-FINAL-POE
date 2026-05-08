using System;
using System.Text.RegularExpressions;

namespace CybersecurityChatbot.Services
{
    // This enum represents different types of user intents.
    // It helps the chatbot understand what the user wants to do.
    public enum NlpIntent
    {
        None,              // No clear intent detected
        AddTask,           // User wants to add a task
        SetReminder,       // User wants to set a reminder
        ShowActivityLog,   // User wants to view activity log
        StartQuiz,         // User wants to start the quiz
        ShowTasks          // User wants to view tasks
    }

    // This class simulates basic Natural Language Processing (NLP).
    // It helps the chatbot understand user input in a flexible way.
    public class NlpService
    {
        // This method detects what the user wants to do (intent detection).
        public NlpIntent DetectIntent(string input)
        {
            // Convert input to lowercase for easier comparison
            string text = input.ToLower();

            // Check for activity log intent
            if (text.Contains("activity log") || text.Contains("what have you done for me"))
                return NlpIntent.ShowActivityLog;

            // Check for quiz intent
            if (text.Contains("start quiz") || text.Contains("play quiz") || text.Contains("open quiz") || text == "quiz")
                return NlpIntent.StartQuiz;

            // Check for showing tasks
            if (text.Contains("show tasks") || text.Contains("view tasks") || text.Contains("my tasks"))
                return NlpIntent.ShowTasks;

            // Check for adding a task (but not reminders)
            if ((text.Contains("add task") || text.Contains("create task") || text.Contains("task to")) && !text.Contains("remind"))
                return NlpIntent.AddTask;

            // Check for reminder intent
            if (text.Contains("remind me") || text.Contains("set reminder") || text.Contains("reminder"))
                return NlpIntent.SetReminder;

            // If nothing matches, return None
            return NlpIntent.None;
        }

        // This method extracts the task title from user input.
        // Example: "add task review privacy settings" → "Review privacy settings"
        public string ExtractTaskTitle(string input)
        {
            string text = input.Trim();

            // List of possible phrases that can start a task command
            string[] prefixes = { "add task", "create task", "task to", "remind me to", "set reminder to", "add a task to" };

            foreach (var prefix in prefixes)
            {
                // Find the position of the prefix in the input
                int idx = text.ToLower().IndexOf(prefix, StringComparison.OrdinalIgnoreCase);

                if (idx >= 0)
                {
                    // Extract the text after the prefix
                    string extracted = text[(idx + prefix.Length)..].Trim(' ', '-', '.', '!');

                    // Remove time-related words like "today", "tomorrow", "in 3 days"
                    extracted = Regex.Replace(extracted, "\\b(today|tomorrow|in \\d+ days?)\\b", "", RegexOptions.IgnoreCase).Trim();

                    // If valid text remains, format it properly
                    if (!string.IsNullOrWhiteSpace(extracted))
                        return char.ToUpper(extracted[0]) + extracted[1..];
                }
            }

            // Return empty if no valid task title found
            return string.Empty;
        }

        // This method extracts a reminder date from user input.
        // Example: "remind me tomorrow" → tomorrow's date
        public DateTime? ExtractReminderDate(string input)
        {
            string text = input.ToLower();

            // If user says "tomorrow"
            if (text.Contains("tomorrow"))
                return DateTime.Today.AddDays(1);

            // Use Regex to find patterns like "in 3 days"
            Match m = Regex.Match(text, @"in\s+(\d+)\s+days?");

            if (m.Success && int.TryParse(m.Groups[1].Value, out int days))
                return DateTime.Today.AddDays(days);

            // Return null if no reminder date found
            return null;
        }
    }
}