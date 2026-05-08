using System.Collections.Generic;
using System.Linq;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    // This service class manages all activity log entries.
    // It allows adding new logs and retrieving existing logs.
    public class ActivityLogService
    {
        // This list stores all activity log entries in memory.
        private readonly List<ActivityLogEntry> _entries = new();

        // This method adds a new activity log entry.
        // It takes the type of action and a description of what happened.
        public void Add(string actionType, string description)
        {
            _entries.Add(new ActivityLogEntry
            {
                // Set the type of action (e.g., "Add Task")
                ActionType = actionType,

                // Set the description of the action
                Description = description
            });
        }

        // This method returns the most recent activity logs.
        // By default, it returns the latest 10 entries.
        public List<ActivityLogEntry> GetRecent(int count = 10)
        {
            return _entries
                // Sort entries by newest first (descending order)
                .OrderByDescending(e => e.Timestamp)

                // Take only the number of entries requested
                .Take(count)

                // Convert the result to a list
                .ToList();
        }

        // This method returns all activity log entries.
        public List<ActivityLogEntry> GetAll()
        {
            return _entries
                // Sort all entries by newest first
                .OrderByDescending(e => e.Timestamp)

                // Convert to a list
                .ToList();
        }

        // This method checks if there are any log entries available.
        public bool HasEntries()
        {
            // Returns true if there is at least one entry
            return _entries.Count > 0;
        }
    }
}