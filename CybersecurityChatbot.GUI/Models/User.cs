using System;
using System.Collections.Generic;

namespace CybersecurityChatbot.Models
{
    // This class represents a user interacting with the chatbot.
    // It stores user details, session information, and conversation tracking.
    public class User
    {
        // Stores the user's name.
        public string Name { get; set; }

        // Stores the date and time when the user session started.
        public DateTime SessionStart { get; set; }

        // Counts how many messages have been exchanged with the chatbot.
        public int MessagesExchanged { get; set; }

        // Stores the user's favorite topic based on interaction.
        public string FavoriteTopic { get; set; } = string.Empty;

        // Stores the last topic discussed with the chatbot.
        public string LastTopic { get; set; } = string.Empty;

        // Stores the current detected sentiment of the user (e.g., happy, neutral, frustrated).
        public string CurrentSentiment { get; set; } = "neutral";

        // Stores the last question asked by the user.
        public string LastQuestion { get; set; } = string.Empty;

        // Stores key-value pairs of information remembered about the user.
        // Example: {"Location" : "South Africa"}
        public Dictionary<string, string> MemoryFacts { get; set; } = new();

        // Default constructor (used when no name is provided).
        // It sets the user name to "Guest" and records the session start time.
        public User()
        {
            Name = "Guest";
            SessionStart = DateTime.Now;
        }

        // Constructor that accepts a name.
        // If the name is empty or invalid, it defaults to "Guest".
        public User(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Guest" : name;
            SessionStart = DateTime.Now;
        }

        // This method increases the message count by 1.
        // It is called each time the user sends a message.
        public void IncrementMessageCount() => MessagesExchanged++;
    }
}