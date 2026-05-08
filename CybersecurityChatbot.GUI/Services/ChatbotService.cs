using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    // This is the main brain of the chatbot.
    // It handles user input, generates responses, detects intent, and manages conversation flow.
    public class ChatbotService
    {
        // Stores all predefined chatbot responses
        private readonly List<Response> _responses;

        // Used to randomly select different responses for variation
        private readonly Random _random;

        // Stores the current user interacting with the chatbot
        private User _currentUser;

        // Constructor initializes responses, random generator, and default user
        public ChatbotService()
        {
            _responses = ResponseBank.GetResponses();
            _random = new Random();
            _currentUser = new User();
        }

        // Sets the current user
        public void SetUser(User user)
        {
            _currentUser = user;
        }

        // Returns the current user
        public User GetCurrentUser()
        {
            return _currentUser;
        }

        // Validates user input (must not be empty and must be at least 2 characters)
        public bool IsValidInput(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Trim().Length >= 2;
        }

        // Main method that processes user input and returns a chatbot response
        public string GetResponse(string userInput)
        {
            // Check if input is valid
            if (!IsValidInput(userInput))
            {
                return "I did not quite understand that. Please type a full question or message.";
            }

            // Normalize input (lowercase + trimmed)
            string normalizedInput = NormalizeInput(userInput);

            // Save the last question asked by the user
            _currentUser.LastQuestion = userInput.Trim();

            // Detect user's mood/sentiment
            DetectSentiment(normalizedInput);

            // Update chatbot memory (favorite topics etc.)
            UpdateMemory(normalizedInput);

            // Check if user is asking about memory
            string memoryResponse = HandleMemoryPrompts(normalizedInput);
            if (!string.IsNullOrWhiteSpace(memoryResponse))
                return memoryResponse;

            // Check if user is asking follow-up questions
            string followUpResponse = HandleFollowUp(normalizedInput);
            if (!string.IsNullOrWhiteSpace(followUpResponse))
                return followUpResponse;

            // Loop through all responses to find matching keywords
            foreach (var response in _responses)
            {
                if (MatchesAnyKeyword(normalizedInput, response.Keywords))
                {
                    // Select a random response
                    string selected = GetRandomResponse(response);

                    // Save last topic discussed
                    if (!string.IsNullOrWhiteSpace(response.Category))
                    {
                        _currentUser.LastTopic = response.Category;
                    }

                    // If exit command, replace username placeholder
                    if (response.Category == "Exit")
                    {
                        return selected.Replace("{userName}", _currentUser.Name);
                    }

                    // Apply sentiment-based response style
                    return ApplySentimentPrefix(selected, response.Category ?? string.Empty);
                }
            }

            // Handle short follow-up questions (like "more", "why")
            if (!string.IsNullOrWhiteSpace(_currentUser.LastTopic) &&
                normalizedInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length <= 4)
            {
                var previousTopicResponse = _responses.FirstOrDefault(r =>
                    string.Equals(r.Category, _currentUser.LastTopic, StringComparison.OrdinalIgnoreCase));

                if (previousTopicResponse != null)
                {
                    return ApplySentimentPrefix(
                        $"It sounds like you are still asking about {_currentUser.LastTopic}. Let me explain further.\n\n{GetRandomResponse(previousTopicResponse)}",
                        _currentUser.LastTopic);
                }
            }

            // Default fallback response
            return ApplySentimentPrefix(GetDefaultResponse(), _currentUser.LastTopic ?? string.Empty);
        }

        // Detects what the user wants to do (intent detection)
        public string DetectIntent(string userInput)
        {
            string input = NormalizeInput(userInput);

            if (ContainsAny(input, "start quiz", "begin quiz", "play quiz", "quiz me"))
                return "start_quiz";

            if (ContainsAny(input, "quit quiz", "exit quiz", "stop quiz", "end quiz"))
                return "quit_quiz";

            if (ContainsAny(input, "show activity log", "activity log"))
                return "show_activity_log";

            if (ContainsAny(input, "show tasks", "view tasks", "task list"))
                return "show_tasks";

            if (ContainsAny(input, "add task", "create task"))
                return "add_task";

            if (ContainsAny(input, "remind me", "set reminder"))
                return "set_reminder";

            return "chat";
        }

        // Converts user input into a TaskItem object
        public TaskItem? ParseTaskFromInput(string userInput)
        {
            string input = NormalizeInput(userInput);

            // Extract task title from "add task"
            if (input.Contains("add task"))
            {
                string title = userInput[(input.IndexOf("add task", StringComparison.OrdinalIgnoreCase) + 8)..]
                    .Trim(' ', '-', ':');

                if (!string.IsNullOrWhiteSpace(title))
                {
                    return new TaskItem
                    {
                        Title = ToTitleCase(title),
                        Description = BuildTaskDescription(title),
                        ReminderDate = null,
                        IsCompleted = false
                    };
                }
            }

            return null;
        }

        // Extracts reminder date from text
        private DateTime? ExtractReminderDate(string input)
        {
            if (input.Contains("tomorrow"))
                return DateTime.Now.Date.AddDays(1);

            if (input.Contains("today"))
                return DateTime.Now;

            return null;
        }

        // Cleans unnecessary words from reminder input
        private string CleanReminderWords(string title)
        {
            return title
                .Replace("tomorrow", "", StringComparison.OrdinalIgnoreCase)
                .Replace("today", "", StringComparison.OrdinalIgnoreCase)
                .Trim();
        }

        // Builds a smart description for tasks
        private string BuildTaskDescription(string title)
        {
            if (title.ToLower().Contains("privacy"))
                return "Review account privacy settings.";

            return $"Complete this cybersecurity task: {title}.";
        }

        // Converts text into Title Case
        private string ToTitleCase(string text)
        {
            return string.Join(" ", text.Split(' ').Select(word => char.ToUpper(word[0]) + word[1..].ToLower()));
        }

        // Normalizes input
        private string NormalizeInput(string input)
        {
            return input.ToLower().Trim();
        }

        // Checks if input matches any keyword using regex
        private bool MatchesAnyKeyword(string input, IEnumerable<string> keywords)
        {
            foreach (string keyword in keywords)
            {
                string pattern = $@"\b{Regex.Escape(keyword)}\b";
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    return true;
            }
            return false;
        }

        // Returns a random response from a list
        private string GetRandomResponse(Response response)
        {
            return response.ResponseOptions[_random.Next(response.ResponseOptions.Count)];
        }

        // Detects user sentiment (mood)
        private void DetectSentiment(string input)
        {
            if (input.Contains("worried")) _currentUser.CurrentSentiment = "worried";
            else if (input.Contains("frustrated")) _currentUser.CurrentSentiment = "frustrated";
            else _currentUser.CurrentSentiment = "neutral";
        }

        // Updates memory based on user input
        private void UpdateMemory(string input)
        {
            if (input.Contains("i like"))
            {
                _currentUser.FavoriteTopic = input;
            }
        }

        // Handles memory questions
        private string HandleMemoryPrompts(string input)
        {
            if (input.Contains("what do you remember"))
            {
                return $"Your name is {_currentUser.Name}.";
            }
            return string.Empty;
        }

        // Handles follow-up questions
        private string HandleFollowUp(string input)
        {
            if (input.Contains("tell me more"))
            {
                return "Here is more information on that topic.";
            }
            return string.Empty;
        }

        // Adjusts response based on user mood
        private string ApplySentimentPrefix(string response, string topic)
        {
            if (_currentUser.CurrentSentiment == "worried")
                return "I understand your concern.\n\n" + response;

            return response;
        }

        // Default fallback response
        private string GetDefaultResponse()
        {
            return "I am not sure I understand. Try asking about cybersecurity topics.";
        }

        // Helper method to check if input contains any phrase
        private bool ContainsAny(string input, params string[] phrases)
        {
            return phrases.Any(p => input.Contains(p));
        }
    }
}