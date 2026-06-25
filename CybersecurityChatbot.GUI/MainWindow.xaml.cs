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
    public partial class MainWindow : Window
    {
        private readonly ChatbotService _chatbotService;
        private readonly DatabaseService _databaseService;
        private readonly ActivityLogService _activityLogService;
        private readonly QuizService _quizService;

        private User _currentUser;
        private TaskItem? _pendingReminderTask;
        private int? _pendingReminderTaskId;

        public MainWindow()
        {
            InitializeComponent();

            _chatbotService = new ChatbotService();
            _databaseService = new DatabaseService();
            _activityLogService = new ActivityLogService();
            _quizService = new QuizService();

            _currentUser = new User("Guest");
            _chatbotService.SetUser(_currentUser);

            SetQuizButtonsVisibility(false);
            PlayGreeting();

            if (_databaseService.EnsureDatabaseSetup())
            {
                AppendBotMessage("Database connection successful.");
                _activityLogService.Add("Database", "Connected to MySQL and verified task table.");
            }
            else
            {
                AppendBotMessage("Database setup could not be completed yet. Please update the MySQL connection string in DatabaseService.cs.");
            }

            AppendBotMessage("Hello! Welcome to the Cybersecurity Awareness Bot.");
            AppendBotMessage("You can chat, manage tasks, start a quiz, or ask for the activity log.");
        }

        private void HeaderBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

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

        private void SetName_Click(object sender, RoutedEventArgs e)
        {
            string name = NameInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            {
                AppendBotMessage("Please enter a valid name with at least 2 characters.");
                return;
            }

            _currentUser = new User(name);
            _chatbotService.SetUser(_currentUser);

            AppendBotMessage($"Welcome, {name}! I will remember your name during our chat.");
            _activityLogService.Add("User", $"User name set to {name}.");
        }

        private void NameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetName_Click(sender, e);
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessInput();
            }
        }

        private void QuickTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                UserInput.Text = button.Content.ToString();
                ProcessInput();
            }
        }

        private void ProcessInput()
        {
            string input = UserInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                AppendBotMessage("Please type something so I can help you.");
                return;
            }

            _currentUser.IncrementMessageCount();
            AppendUserMessage(input);
            _activityLogService.Add("Chat", $"User entered: {input}");

            if (_pendingReminderTask != null && TryHandleReminderFollowUp(input))
            {
                UserInput.Clear();
                UserInput.Focus();
                return;
            }

            string intent = _chatbotService.DetectIntent(input);

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

            UserInput.Clear();
            UserInput.Focus();
        }

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

        private bool TryHandleReminderFollowUp(string input)
        {
            string normalized = input.Trim().ToLower();

            if (_pendingReminderTask == null || !_pendingReminderTaskId.HasValue)
                return false;

            if (normalized == "no" || normalized.Contains("no reminder"))
            {
                AppendBotMessage("Okay, no reminder was set.");
                _activityLogService.Add("Reminder", $"No reminder set for task: {_pendingReminderTask.Title}");

                _pendingReminderTask = null;
                _pendingReminderTaskId = null;
                return true;
            }

            if (normalized == "yes" || normalized == "yes please")
            {
                AppendBotMessage("Sure. Tell me when to remind you, for example: 'tomorrow' or 'in 3 days'.");
                return true;
            }

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

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleTextBox.Text.Trim();
            string description = TaskDescriptionTextBox.Text.Trim();
            DateTime? reminderDate = TaskReminderDatePicker.SelectedDate;

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

        private void ViewTasks_Click(object sender, RoutedEventArgs e)
        {
            ViewTasksInternal();
        }

        private void ViewTasksInternal()
        {
            try
            {
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

        private void MarkTaskComplete_Click(object sender, RoutedEventArgs e)
        {
            ViewTasksInternal();
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            ViewTasksInternal();
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            StartQuizInternal();
        }

        private void StartQuizInternal()
        {
            _quizService.StartQuiz();
            _activityLogService.Add("Quiz", "Quiz started.");
            AppendBotMessage("Quiz started. Answer using the quiz buttons below.");
            SetQuizButtonsVisibility(true);
            ShowCurrentQuizQuestion();
        }

        private void QuitQuiz_Click(object sender, RoutedEventArgs e)
        {
            QuitQuiz();
        }

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

        private void ShowCurrentQuizQuestion()
        {
            var question = _quizService.GetCurrentQuestion();

            if (question == null)
            {
                QuizQuestionText.Text = "Quiz finished.";
                SetQuizButtonsVisibility(false);
                return;
            }

            QuizQuestionText.Text = $"Question {_quizService.GetQuestionNumber()} of {_quizService.GetTotalQuestions()}:\n{question.QuestionText}";

            QuizOption1.Content = question.Options.Count > 0 ? question.Options[0] : "";
            QuizOption2.Content = question.Options.Count > 1 ? question.Options[1] : "";
            QuizOption3.Content = question.Options.Count > 2 ? question.Options[2] : "";
            QuizOption4.Content = question.Options.Count > 3 ? question.Options[3] : "";

            QuizOption1.Tag = 0;
            QuizOption2.Tag = 1;
            QuizOption3.Tag = 2;
            QuizOption4.Tag = 3;

            QuizOption1.Visibility = question.Options.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            QuizOption2.Visibility = question.Options.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            QuizOption3.Visibility = question.Options.Count > 2 ? Visibility.Visible : Visibility.Collapsed;
            QuizOption4.Visibility = question.Options.Count > 3 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void QuizOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int optionIndex)
            {
                string feedback = _quizService.SubmitAnswer(optionIndex);
                AppendBotMessage(feedback);
                _activityLogService.Add("Quiz", $"Answered quiz question. Current score: {_quizService.Score}");

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

        private void SetQuizButtonsVisibility(bool visible)
        {
            var state = visible ? Visibility.Visible : Visibility.Collapsed;
            QuizOption1.Visibility = state;
            QuizOption2.Visibility = state;
            QuizOption3.Visibility = state;
            QuizOption4.Visibility = state;
        }

        private void ShowMemory_Click(object sender, RoutedEventArgs e)
        {
            AppendBotMessage(_chatbotService.GetResponse("what do you remember"));
            _activityLogService.Add("Memory", "Displayed remembered user information.");
        }

        private void ShowActivityLog_Click(object sender, RoutedEventArgs e)
        {
            ShowActivityLogInternal();
        }

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

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            AppendBotMessage("Chat cleared. We can continue from here.");
            _activityLogService.Add("UI", "Chat cleared.");
        }

        private void AppendUserMessage(string message)
        {
            ChatPanel.Children.Add(CreateMessageBubble($"{_currentUser.Name}: {message}", false));
            ScrollToBottom();
        }

        private void AppendBotMessage(string message)
        {
            ChatPanel.Children.Add(CreateMessageBubble($"BOT: {message}", true));
            ScrollToBottom();
        }

        private Border CreateMessageBubble(string text, bool isBot)
        {
            return new Border
            {
                Background = isBot
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F766E")),
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

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }
    }
}