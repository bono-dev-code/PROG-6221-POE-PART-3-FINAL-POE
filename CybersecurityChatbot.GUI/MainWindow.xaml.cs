using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CybersecurityChatbot.Models;
using CybersecurityChatbot.Services;

namespace CybersecurityChatbot.GUI
{
    // This is the main window of the WPF chatbot application.
    // It connects the GUI buttons, chatbot logic, database, quiz, tasks, and activity log together.
    public partial class MainWindow : Window
    {
        // These are the services that handle different parts of the app
        private readonly ChatbotService _chatbotService;      // Handles chat responses
        private readonly DatabaseService _databaseService;    // Handles database operations
        private readonly ActivityLogService _activityLogService; // Tracks user actions
        private readonly QuizService _quizService;            // Handles quiz functionality

        // Track the current user and any pending reminder tasks
        private User _currentUser;
        private TaskItem? _pendingReminderTask;    // Stores a task waiting for reminder confirmation
        private int? _pendingReminderTaskId;       // Stores the ID of pending task

        // Constructor - runs when the window first opens
        public MainWindow()
        {
            InitializeComponent(); // Loads the XAML design

            // Create instances of all the services
            _chatbotService = new ChatbotService();
            _databaseService = new DatabaseService();
            _activityLogService = new ActivityLogService();
            _quizService = new QuizService();

            // Create a default guest user
            _currentUser = new User("Guest");
            _chatbotService.SetUser(_currentUser);

            SetQuizButtonsVisibility(false); // Hide quiz buttons until quiz starts
            PlayGreeting(); // Play welcome sound

            // Check if database is working
            if (_databaseService.EnsureDatabaseSetup())
            {
                AppendBotMessage("Database connection successful.");
                _activityLogService.Add("Database", "Connected to MySQL and verified task table.");
            }
            else
            {
                AppendBotMessage("Database setup could not be completed yet. Please update the MySQL connection string in DatabaseService.cs.");
            }

            // Welcome messages
            AppendBotMessage("Hello! Welcome to the Cybersecurity Awareness Bot.");
            AppendBotMessage("You can chat, manage tasks, start a quiz, or ask for the activity log.");
        }

        // Allows user to drag the window by clicking on the header
        private void HeaderBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // Plays greeting sound when app starts
        private void PlayGreeting()
        {
            try
            {
                string audioPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Resources",
                    "greeting.wav");

                if (File.Exists(audioPath))
                {
                    AudioService audioService = new AudioService(audioPath);
                    audioService.PlayGreeting();
                }
                else
                {
                    AppendBotMessage($"Audio file not found: {audioPath}");
                }
            }
            catch (Exception ex)
            {
                AppendBotMessage($"Audio failed to play: {ex.Message}");
            }
        }

        // Sets the user's name when they click the "Set Name" button
        private void SetName_Click(object sender, RoutedEventArgs e)
        {
            string name = NameInput.Text.Trim();

            // Validate the name (must be at least 2 characters)
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            {
                AppendBotMessage("Please enter a valid name with at least 2 characters.");
                return;
            }

            // Create new user with the entered name
            _currentUser = new User(name);
            _chatbotService.SetUser(_currentUser);

            AppendBotMessage($"Welcome, {name}! I will remember your name during our chat.");
            _activityLogService.Add("User", $"User name set to {name}.");
        }

        // Handles clicking the Send button
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        // Handles pressing Enter key to send message
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessInput();
            }
        }

        // Handles clicking on quick topic buttons
        private void QuickTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                UserInput.Text = button.Content.ToString();
                ProcessInput();
            }
        }

        // Main method that processes user input and decides what to do
        private void ProcessInput()
        {
            string input = UserInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                AppendBotMessage("Please type something so I can help you.");
                return;
            }

            // Track user activity
            _currentUser.IncrementMessageCount();
            AppendUserMessage(input);
            _activityLogService.Add("Chat", $"User entered: {input}");

            // Check if we're waiting for a reminder follow-up
            if (_pendingReminderTask != null && TryHandleReminderFollowUp(input))
            {
                UserInput.Clear();
                UserInput.Focus();
                return;
            }

            // Figure out what the user wants (intent detection)
            string intent = _chatbotService.DetectIntent(input);

            // Handle different intents
            switch (intent)
            {
                case "start_quiz":
                    StartQuizInternal();
                    break;

                case "quit_quiz":
                    QuitQuiz();
                    break;

                case "show_activity_log":
                    ShowActivityLogInternal();
                    break;

                case "show_tasks":
                    ViewTasksInternal();
                    break;

                case "add_task":
                case "set_reminder":
                    HandleTaskIntent(input);
                    break;

                default:
                    string response = _chatbotService.GetResponse(input);
                    AppendBotMessage(response);
                    break;
            }

            // Clear input and refocus for next message
            UserInput.Clear();
            UserInput.Focus();
        }

        // Processes task-related user input
        private void HandleTaskIntent(string input)
        {
            var task = _chatbotService.ParseTaskFromInput(input);

            if (task == null || string.IsNullOrWhiteSpace(task.Title))
            {
                AppendBotMessage("I could not understand the task fully. Try something like 'Add task - Enable 2FA' or 'Remind me to update my password tomorrow'.");
                return;
            }

            try
            {
                // If the task already has a reminder date, save it directly
                if (task.ReminderDate.HasValue)
                {
                    int taskId = _databaseService.AddTask(task);

                    AppendBotMessage($"Task added: {task.Title}. Reminder set for {task.ReminderDate:yyyy-MM-dd}.");
                    _activityLogService.Add("Task", $"Task added: {task.Title}");
                    _activityLogService.Add("Reminder", $"Reminder set for task '{task.Title}' on {task.ReminderDate:yyyy-MM-dd}");

                    _pendingReminderTask = null;
                    _pendingReminderTaskId = null;
                    return;
                }

                // If no reminder date, save task and ask if user wants a reminder
                int newTaskId = _databaseService.AddTask(task);

                _pendingReminderTask = task;
                _pendingReminderTaskId = newTaskId;

                AppendBotMessage($"Task added with the description \"{task.Description}\". Would you like a reminder?");
                _activityLogService.Add("Task", $"Task added: {task.Title}");
            }
            catch (Exception ex)
            {
                AppendBotMessage("There was a problem saving the task to the database.");
                _activityLogService.Add("Error", $"Task save failed: {ex.Message}");
            }
        }

        // Handles follow-up questions about setting reminders
        private bool TryHandleReminderFollowUp(string input)
        {
            string normalized = input.Trim().ToLower();

            if (_pendingReminderTask == null || !_pendingReminderTaskId.HasValue)
                return false;

            // User says no to reminder
            if (normalized == "no" || normalized.Contains("no reminder"))
            {
                AppendBotMessage("Okay, no reminder was set.");
                _activityLogService.Add("Reminder", $"No reminder set for task: {_pendingReminderTask.Title}");

                _pendingReminderTask = null;
                _pendingReminderTaskId = null;
                return true;
            }

            // User says yes to reminder - ask for when
            if (normalized == "yes" || normalized == "yes please")
            {
                AppendBotMessage("Sure. Tell me when to remind you, for example: 'tomorrow' or 'in 3 days'.");
                return true;
            }

            // Parse reminder date from user input
            DateTime? reminderDate = null;

            if (normalized.Contains("tomorrow"))
                reminderDate = DateTime.Now.Date.AddDays(1);
            else if (normalized.Contains("today"))
                reminderDate = DateTime.Now;
            else if (normalized.Contains("in 1 day"))
                reminderDate = DateTime.Now.Date.AddDays(1);
            else if (normalized.Contains("in 2 days"))
                reminderDate = DateTime.Now.Date.AddDays(2);
            else if (normalized.Contains("in 3 days"))
                reminderDate = DateTime.Now.Date.AddDays(3);
            else if (normalized.Contains("in 5 days"))
                reminderDate = DateTime.Now.Date.AddDays(5);
            else if (normalized.Contains("in 7 days"))
                reminderDate = DateTime.Now.Date.AddDays(7);

            // If we got a valid date, save the reminder
            if (reminderDate.HasValue)
            {
                _databaseService.UpdateTaskReminder(_pendingReminderTaskId.Value, reminderDate.Value);

                int days = Math.Max(0, (reminderDate.Value.Date - DateTime.Now.Date).Days);

                if (normalized.Contains("tomorrow"))
                    AppendBotMessage("Got it! I'll remind you tomorrow.");
                else if (normalized.Contains("today"))
                    AppendBotMessage("Got it! I'll remind you today.");
                else
                    AppendBotMessage($"Got it! I'll remind you in {days} days.");

                _activityLogService.Add("Reminder", $"Reminder set for task '{_pendingReminderTask.Title}' on {reminderDate:yyyy-MM-dd}");

                _pendingReminderTask = null;
                _pendingReminderTaskId = null;
                return true;
            }

            if (normalized.StartsWith("yes"))
            {
                AppendBotMessage("Sure. Tell me when to remind you, for example: 'in 3 days' or 'tomorrow'.");
                return true;
            }

            return false;
        }

        // Adds a demo task when the Add Task button is clicked
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var task = new TaskItem
            {
                Title = "Enable two-factor authentication",
                Description = "Enable two-factor authentication to add an extra layer of security.",
                ReminderDate = DateTime.Now.AddDays(3),
                IsCompleted = false
            };

            try
            {
                _databaseService.AddTask(task);
                AppendBotMessage("Task added successfully: Enable two-factor authentication. Reminder set for 3 days.");
                _activityLogService.Add("Task", "Demo task added with a 3-day reminder.");
            }
            catch (Exception ex)
            {
                AppendBotMessage("There was a problem saving the demo task.");
                _activityLogService.Add("Error", $"Demo task save failed: {ex.Message}");
            }
        }

        // Shows tasks when View Tasks button is clicked
        private void ViewTasks_Click(object sender, RoutedEventArgs e)
        {
            ViewTasksInternal();
        }

        // Loads and displays all saved tasks from the database
        private void ViewTasksInternal()
        {
            try
            {
                var tasks = _databaseService.GetTasks();

                if (!tasks.Any())
                {
                    AppendBotMessage("There are currently no tasks saved.");
                    return;
                }

                AppendBotMessage("Here are your saved tasks:");

                // Loop through each task and display its information
                foreach (var task in tasks)
                {
                    string reminderText = task.ReminderDate.HasValue
                        ? task.ReminderDate.Value.ToString("yyyy-MM-dd")
                        : "No reminder";

                    // Converts the database true/false value into clear words for the user
                    string statusText = task.IsCompleted
                        ? "✅ Task Completed"
                        : "❌ Task Not Completed";

                    AppendBotMessage(
                        $"Task #{task.Id}\n" +
                        $"Title: {task.Title}\n" +
                        $"Description: {task.Description}\n" +
                        $"Reminder: {reminderText}\n" +
                        $"Status: {statusText}"
                    );
                }

                _activityLogService.Add("Task", "Viewed saved tasks with completion status.");
            }
            catch (Exception ex)
            {
                AppendBotMessage("I could not load tasks from the database.");
                _activityLogService.Add("Error", $"Task load failed: {ex.Message}");
            }
        }

        // Marks a task as completed
        private void MarkTaskComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetTaskId(out int taskId))
            {
                AppendBotMessage("Please enter a valid numeric Task ID.");
                return;
            }

            try
            {
                _databaseService.MarkTaskCompleted(taskId);
                AppendBotMessage($"✅ Task #{taskId} marked as completed.");
                _activityLogService.Add("Task", $"Task #{taskId} marked as completed.");
            }
            catch (Exception ex)
            {
                AppendBotMessage("I could not mark the task as completed.");
                _activityLogService.Add("Error", $"Task complete failed: {ex.Message}");
            }
        }

        // Deletes a task
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetTaskId(out int taskId))
            {
                AppendBotMessage("Please enter a valid numeric Task ID.");
                return;
            }

            try
            {
                _databaseService.DeleteTask(taskId);
                AppendBotMessage($"🗑️ Task #{taskId} deleted successfully.");
                _activityLogService.Add("Task", $"Task #{taskId} deleted.");
            }
            catch (Exception ex)
            {
                AppendBotMessage("I could not delete the task.");
                _activityLogService.Add("Error", $"Task delete failed: {ex.Message}");
            }
        }

        // Gets the task ID from the text box
        private bool TryGetTaskId(out int taskId)
        {
            return int.TryParse(TaskIdTextBox.Text.Trim(), out taskId);
        }

        // Starts the quiz when button is clicked
        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            StartQuizInternal();
        }

        // Internal method to start the quiz
        private void StartQuizInternal()
        {
            _quizService.StartQuiz();
            _activityLogService.Add("Quiz", "Quiz started.");
            AppendBotMessage("Quiz started. Answer using the quiz buttons below.");
            SetQuizButtonsVisibility(true);
            ShowCurrentQuizQuestion();
        }

        // Quits the quiz
        private void QuitQuiz_Click(object sender, RoutedEventArgs e)
        {
            QuitQuiz();
        }

        // Internal method to quit the quiz
        private void QuitQuiz()
        {
            if (_quizService.IsQuizActive)
            {
                _quizService.ForceStop();
                SetQuizButtonsVisibility(false);
                QuizQuestionText.Text = "Quiz stopped.";
                AppendBotMessage("Quiz ended. You can start again anytime.");
                _activityLogService.Add("Quiz", "User quit the quiz early.");
            }
            else
            {
                AppendBotMessage("No quiz is currently running.");
            }
        }

        // Displays the current quiz question
        private void ShowCurrentQuizQuestion()
        {
            var question = _quizService.GetCurrentQuestion();

            if (question == null)
            {
                QuizQuestionText.Text = "Quiz finished.";
                SetQuizButtonsVisibility(false);
                return;
            }

            // Show question text
            QuizQuestionText.Text = $"Question {_quizService.GetQuestionNumber()} of {_quizService.GetTotalQuestions()}:\n{question.QuestionText}";

            // Set option button text
            QuizOption1.Content = question.Options.Count > 0 ? question.Options[0] : "";
            QuizOption2.Content = question.Options.Count > 1 ? question.Options[1] : "";
            QuizOption3.Content = question.Options.Count > 2 ? question.Options[2] : "";
            QuizOption4.Content = question.Options.Count > 3 ? question.Options[3] : "";

            // Store option index in button's Tag property
            QuizOption1.Tag = 0;
            QuizOption2.Tag = 1;
            QuizOption3.Tag = 2;
            QuizOption4.Tag = 3;

            // Hide buttons that don't have options
            QuizOption1.Visibility = question.Options.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            QuizOption2.Visibility = question.Options.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            QuizOption3.Visibility = question.Options.Count > 2 ? Visibility.Visible : Visibility.Collapsed;
            QuizOption4.Visibility = question.Options.Count > 3 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Handles clicking a quiz answer button
        private void QuizOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int optionIndex)
            {
                string feedback = _quizService.SubmitAnswer(optionIndex);
                AppendBotMessage(feedback);
                _activityLogService.Add("Quiz", $"Answered quiz question. Current score: {_quizService.Score}");

                if (_quizService.IsQuizActive)
                {
                    ShowCurrentQuizQuestion(); // Show next question
                }
                else
                {
                    QuizQuestionText.Text = "Quiz complete. Start again any time.";
                    SetQuizButtonsVisibility(false);
                    _activityLogService.Add("Quiz", $"Quiz completed with score {_quizService.Score}/{_quizService.GetTotalQuestions()}");
                }
            }
        }

        // Shows or hides the quiz buttons
        private void SetQuizButtonsVisibility(bool visible)
        {
            var state = visible ? Visibility.Visible : Visibility.Collapsed;
            QuizOption1.Visibility = state;
            QuizOption2.Visibility = state;
            QuizOption3.Visibility = state;
            QuizOption4.Visibility = state;
        }

        // Shows what the bot remembers about the user
        private void ShowMemory_Click(object sender, RoutedEventArgs e)
        {
            AppendBotMessage(_chatbotService.GetResponse("what do you remember"));
            _activityLogService.Add("Memory", "Displayed remembered user information.");
        }

        // Shows the activity log
        private void ShowActivityLog_Click(object sender, RoutedEventArgs e)
        {
            ShowActivityLogInternal();
        }

        // Internal method to show activity log
        private void ShowActivityLogInternal()
        {
            if (!_activityLogService.HasEntries())
            {
                AppendBotMessage("There is no activity log yet.");
                return;
            }

            AppendBotMessage("Here is a summary of recent actions:");

            foreach (var entry in _activityLogService.GetRecent(10))
            {
                AppendBotMessage(entry.ToString());
            }
        }

        // Clears all messages from the chat
        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            AppendBotMessage("Chat cleared. We can continue from here.");
            _activityLogService.Add("UI", "Chat cleared.");
        }

        // Adds a user message to the chat display
        private void AppendUserMessage(string message)
        {
            ChatPanel.Children.Add(CreateMessageBubble($"{_currentUser.Name}: {message}", false));
            ScrollToBottom();
        }

        // Adds a bot message to the chat display
        private void AppendBotMessage(string message)
        {
            ChatPanel.Children.Add(CreateMessageBubble($"BOT: {message}", true));
            ScrollToBottom();
        }

        // Creates a styled message bubble for chat display
        private Border CreateMessageBubble(string text, bool isBot)
        {
            return new Border
            {
                Background = isBot
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"))  // Dark blue-gray for bot
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F766E")), // Teal for user
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = isBot ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                MaxWidth = 720,
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14
                }
            };
        }

        // Scrolls the chat view to show the newest message
        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }
    }
}