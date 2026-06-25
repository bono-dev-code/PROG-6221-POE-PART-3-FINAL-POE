using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CybersecurityChatbot.Models;
using CybersecurityChatbot.Services;

namespace CybersecurityChatbot.GUI
{
    // Main window of the cybersecurity chatbot application
    public partial class MainWindow : Window
    {
        // Service objects that handle different parts of the app
        private readonly ChatbotService _chatbotService;    // Handles chat responses
        private readonly DatabaseService _databaseService;  // Saves/loads tasks from database
        private readonly ActivityLogService _activityLogService; // Tracks user actions
        private readonly QuizService _quizService;          // Manages quiz questions

        // Track the current user and any pending reminder
        private User _currentUser;
        private TaskItem? _pendingReminderTask;     // A task waiting for a reminder
        private int? _pendingReminderTaskId;        // Database ID of that task

        // Constructor - runs when the window first opens
        public MainWindow()
        {
            InitializeComponent(); // Sets up all the UI elements

            // Create service objects
            _chatbotService = new ChatbotService();
            _databaseService = new DatabaseService();
            _activityLogService = new ActivityLogService();
            _quizService = new QuizService();

            // Set default user as "Guest"
            _currentUser = new User("Guest");
            _chatbotService.SetUser(_currentUser);

            // Hide quiz buttons until needed
            SetQuizButtonsVisibility(false);
            PlayGreeting(); // Play welcome sound

            // Try connecting to database and show status
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

        // Allows dragging the window by clicking on the header
        private void HeaderBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove(); // Built-in WPF method to move the window
            }
        }

        // Plays the greeting sound file if it exists
        private void PlayGreeting()
        {
            try
            {
                // Find the audio file in the Resources folder
                string audioPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Resources",
                    "greeting.wav");

                if (File.Exists(audioPath))
                {
                    // Play the audio using our AudioService
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

        // Sets the user's name from the input field
        private void SetName_Click(object sender, RoutedEventArgs e)
        {
            string name = NameInput.Text.Trim();

            // Validate the name (must have at least 2 characters)
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            {
                AppendBotMessage("Please enter a valid name with at least 2 characters.");
                return;
            }

            // Create new user object and update chatbot
            _currentUser = new User(name);
            _chatbotService.SetUser(_currentUser);

            AppendBotMessage($"Welcome, {name}! I will remember your name during our chat.");
            _activityLogService.Add("User", $"User name set to {name}.");
        }

        // Allows pressing Enter to set the name
        private void NameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetName_Click(sender, e);
            }
        }

        // Handles Send button click
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        // Allows pressing Enter to send a message
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
                // Put button text into input field and process it
                UserInput.Text = button.Content.ToString();
                ProcessInput();
            }
        }

        // Main method to process user input
        private void ProcessInput()
        {
            string input = UserInput.Text.Trim();

            // Check if user typed anything
            if (string.IsNullOrWhiteSpace(input))
            {
                AppendBotMessage("Please type something so I can help you.");
                return;
            }

            // Update user stats and show their message
            _currentUser.IncrementMessageCount();
            AppendUserMessage(input);
            _activityLogService.Add("Chat", $"User entered: {input}");

            // Check if we're expecting a reminder follow-up
            if (_pendingReminderTask != null && TryHandleReminderFollowUp(input))
            {
                UserInput.Clear();
                UserInput.Focus();
                return;
            }

            // Figure out what the user wants using chatbot intent detection
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
                    // Regular chat response
                    string response = _chatbotService.GetResponse(input);
                    AppendBotMessage(response);
                    break;
            }

            UserInput.Clear();
            UserInput.Focus();
        }

        // Handles adding tasks from chat input
        private void HandleTaskIntent(string input)
        {
            // Parse task from the input text
            var task = _chatbotService.ParseTaskFromInput(input);

            // Make sure we understood the task
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
                    _databaseService.AddTask(task);

                    AppendBotMessage($"Task added: {task.Title}. Reminder set for {task.ReminderDate:yyyy-MM-dd}.");
                    _activityLogService.Add("Task", $"Task added: {task.Title}");
                    _activityLogService.Add("Reminder", $"Reminder set for task '{task.Title}' on {task.ReminderDate:yyyy-MM-dd}");

                    _pendingReminderTask = null;
                    _pendingReminderTaskId = null;
                    return;
                }

                // Save task without reminder and ask if they want one
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

        // Handles follow-up conversation about reminders
        private bool TryHandleReminderFollowUp(string input)
        {
            string normalized = input.Trim().ToLower();

            if (_pendingReminderTask == null || !_pendingReminderTaskId.HasValue)
                return false;

            // User says "no" - don't set a reminder
            if (normalized == "no" || normalized.Contains("no reminder"))
            {
                AppendBotMessage("Okay, no reminder was set.");
                _activityLogService.Add("Reminder", $"No reminder set for task: {_pendingReminderTask.Title}");

                _pendingReminderTask = null;
                _pendingReminderTaskId = null;
                return true;
            }

            // User says "yes" - ask when
            if (normalized == "yes" || normalized == "yes please")
            {
                AppendBotMessage("Sure. Tell me when to remind you, for example: 'tomorrow' or 'in 3 days'.");
                return true;
            }

            // Try to parse reminder date from user's response
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

            // If we got a valid date, update the task
            if (reminderDate.HasValue)
            {
                _databaseService.UpdateTaskReminder(_pendingReminderTaskId.Value, reminderDate.Value);

                int days = Math.Max(0, (reminderDate.Value.Date - DateTime.Now.Date).Days);

                // Give user-friendly confirmation
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

            // User said something starting with "yes" but we didn't understand the date
            if (normalized.StartsWith("yes"))
            {
                AppendBotMessage("Sure. Tell me when to remind you, for example: 'in 3 days' or 'tomorrow'.");
                return true;
            }

            return false;
        }

        // Adds a task from the manual input fields (not chat)
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleTextBox.Text.Trim();
            string description = TaskDescriptionTextBox.Text.Trim();
            DateTime? reminderDate = TaskReminderDatePicker.SelectedDate;

            // Validate input
            if (string.IsNullOrWhiteSpace(title))
            {
                AppendBotMessage("Please enter a task title before adding the task.");
                TaskTitleTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                AppendBotMessage("Please enter a task description before adding the task.");
                TaskDescriptionTextBox.Focus();
                return;
            }

            // Create task object
            var task = new TaskItem
            {
                Title = title,
                Description = description,
                ReminderDate = reminderDate,
                IsCompleted = false
            };

            try
            {
                _databaseService.AddTask(task);

                string reminderText = reminderDate.HasValue
                    ? $" Reminder set for {reminderDate.Value:yyyy-MM-dd}."
                    : " No reminder set.";

                AppendBotMessage($"Task added successfully: {title}.{reminderText}");
                _activityLogService.Add("Task", $"Task added: '{title}'.{reminderText}");

                // Clear input fields
                TaskTitleTextBox.Clear();
                TaskDescriptionTextBox.Clear();
                TaskReminderDatePicker.SelectedDate = null;
            }
            catch (Exception ex)
            {
                AppendBotMessage("There was a problem saving the task. Please check your database connection.");
                _activityLogService.Add("Error", $"Task save failed: {ex.Message}");
            }
        }

        // Opens the Task Manager window
        private void ViewTasks_Click(object sender, RoutedEventArgs e)
        {
            ViewTasksInternal();
        }

        // Internal method to open Task Manager
        private void ViewTasksInternal()
        {
            try
            {
                // Create and show the task window
                TaskManagerWindow taskWindow = new TaskManagerWindow(_databaseService, _activityLogService);
                taskWindow.Owner = this;

                taskWindow.Show();

                AppendBotMessage("Task Manager opened. You can view, complete, or delete saved tasks there.");
                _activityLogService.Add("Task", "Opened Task Manager window.");
            }
            catch (Exception ex)
            {
                AppendBotMessage("I could not open the Task Manager window.");
                _activityLogService.Add("Error", $"Task Manager failed: {ex.Message}");
            }
        }

        // These buttons just open the Task Manager window
        private void MarkTaskComplete_Click(object sender, RoutedEventArgs e)
        {
            ViewTasksInternal();
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            ViewTasksInternal();
        }

        // Starts the quiz
        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            StartQuizInternal();
        }

        // Internal method to start quiz
        private void StartQuizInternal()
        {
            _quizService.StartQuiz();
            _activityLogService.Add("Quiz", "Quiz started.");
            AppendBotMessage("Quiz started. Answer using the quiz buttons below.");
            SetQuizButtonsVisibility(true); // Show quiz buttons
            ShowCurrentQuizQuestion();      // Display first question
        }

        // Quits the quiz
        private void QuitQuiz_Click(object sender, RoutedEventArgs e)
        {
            QuitQuiz();
        }

        // Internal method to quit quiz
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

            // Check if quiz is finished
            if (question == null)
            {
                QuizQuestionText.Text = "Quiz finished.";
                SetQuizButtonsVisibility(false);
                return;
            }

            // Show question with number
            QuizQuestionText.Text = $"Question {_quizService.GetQuestionNumber()} of {_quizService.GetTotalQuestions()}:\n{question.QuestionText}";

            // Set button text to options
            QuizOption1.Content = question.Options.Count > 0 ? question.Options[0] : "";
            QuizOption2.Content = question.Options.Count > 1 ? question.Options[1] : "";
            QuizOption3.Content = question.Options.Count > 2 ? question.Options[2] : "";
            QuizOption4.Content = question.Options.Count > 3 ? question.Options[3] : "";

            // Store option index in button Tag
            QuizOption1.Tag = 0;
            QuizOption2.Tag = 1;
            QuizOption3.Tag = 2;
            QuizOption4.Tag = 3;

            // Hide buttons if fewer options
            QuizOption1.Visibility = question.Options.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            QuizOption2.Visibility = question.Options.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            QuizOption3.Visibility = question.Options.Count > 2 ? Visibility.Visible : Visibility.Collapsed;
            QuizOption4.Visibility = question.Options.Count > 3 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Handles clicking a quiz option button
        private void QuizOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int optionIndex)
            {
                // Submit answer and get feedback
                string feedback = _quizService.SubmitAnswer(optionIndex);
                AppendBotMessage(feedback);
                _activityLogService.Add("Quiz", $"Answered quiz question. Current score: {_quizService.Score}");

                // Show next question or finish quiz
                if (_quizService.IsQuizActive)
                {
                    ShowCurrentQuizQuestion();
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

        // Shows what the chatbot remembers about the user
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

            // Show last 10 log entries
            foreach (var entry in _activityLogService.GetRecent(10))
            {
                AppendBotMessage(entry.ToString());
            }
        }

        // Clears all messages from the chat panel
        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            AppendBotMessage("Chat cleared. We can continue from here.");
            _activityLogService.Add("UI", "Chat cleared.");
        }

        // Adds a user message bubble to the chat
        private void AppendUserMessage(string message)
        {
            ChatPanel.Children.Add(CreateMessageBubble($"{_currentUser.Name}: {message}", false));
            ScrollToBottom();
        }

        // Adds a bot message bubble to the chat
        private void AppendBotMessage(string message)
        {
            ChatPanel.Children.Add(CreateMessageBubble($"BOT: {message}", true));
            ScrollToBottom();
        }

        // Creates a styled message bubble for the chat
        private Border CreateMessageBubble(string text, bool isBot)
        {
            return new Border
            {
                Background = isBot
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")) // Dark blue-gray for bot
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F766E")), // Teal for user
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = isBot ? HorizontalAlignment.Left : HorizontalAlignment.Right, // Bot on left, user on right
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

        // Scrolls the chat view to show the latest message
        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }
    }
}