using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    // This is the main brain of the chatbot.
    // It handles user input, keyword recognition, NLP simulation, memory, sentiment, and conversation flow.
    public class ChatbotService
    {
        // Stores all predefined chatbot responses from ResponseBank.
        private readonly List<Response> _responses;

        // Used to randomly select different responses so the bot does not sound repetitive.
        private readonly Random _random;

        // Stores the current user and memory details such as name, last topic, and sentiment.
        private User _currentUser;

        // Constructor initializes responses, random generator, and default user.
        public ChatbotService()
        {
            _responses = ResponseBank.GetResponses();
            _random = new Random();
            _currentUser = new User();
        }

        // Sets the current user from the GUI.
        public void SetUser(User user)
        {
            _currentUser = user;
        }

        // Returns the current user.
        public User GetCurrentUser()
        {
            return _currentUser;
        }

        // Validates user input so empty or very short messages do not crash the bot.
        public bool IsValidInput(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Trim().Length >= 2;
        }

        // Main method that processes user input and returns a chatbot response.
        public string GetResponse(string userInput)
        {
            // Check if input is valid before processing.
            if (!IsValidInput(userInput))
            {
                return "I did not quite understand that. Please type a full question or message.";
            }

            // Normalize input by making it lowercase and removing extra spaces.
            string normalizedInput = NormalizeInput(userInput);

            // Store the user's last question for memory and conversation flow.
            _currentUser.LastQuestion = userInput.Trim();

            // Detect user's mood/sentiment first so replies can sound more human.
            DetectSentiment(normalizedInput);

            // Update chatbot memory, for example favourite cybersecurity topic.
            UpdateMemory(normalizedInput);

            // Handle memory questions such as "what do you remember?".
            string memoryResponse = HandleMemoryPrompts(normalizedInput);
            if (!string.IsNullOrWhiteSpace(memoryResponse))
            {
                return memoryResponse;
            }

            // Handle follow-up questions such as "tell me more" and "another tip".
            // This is checked before topic matching so the bot continues the previous topic.
            string followUpResponse = HandleFollowUp(normalizedInput);
            if (!string.IsNullOrWhiteSpace(followUpResponse))
            {
                return followUpResponse;
            }

            // Smart NLP-style intent detection.
            // This fixes the confusion problem by checking specific meanings before general keywords.
            string smartIntentResponse = DetectSmartCybersecurityIntent(normalizedInput);
            if (!string.IsNullOrWhiteSpace(smartIntentResponse))
            {
                return smartIntentResponse;
            }

            // Normal ResponseBank keyword matching.
            // This happens after smart intent detection so broad keywords do not override specific questions.
            foreach (var response in _responses)
            {
                if (MatchesAnyKeyword(normalizedInput, response.Keywords))
                {
                    string selected = GetRandomResponse(response);

                    // Save last topic so follow-up questions can continue the same topic.
                    if (!string.IsNullOrWhiteSpace(response.Category))
                    {
                        _currentUser.LastTopic = response.Category;
                    }

                    // If the user exits, replace the name placeholder.
                    if (response.Category == "Exit")
                    {
                        return selected.Replace("{userName}", _currentUser.Name);
                    }

                    // Apply sentiment tone if the user is worried, frustrated, confused, etc.
                    return ApplySentimentPrefix(selected, response.Category ?? string.Empty);
                }
            }

            // Extra short follow-up support for short messages like "why?", "how?", "more", etc.
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

            // Default fallback response for unsupported input.
            return ApplySentimentPrefix(GetDefaultResponse(), _currentUser.LastTopic ?? string.Empty);
        }

        // Detects app-level intents used by the GUI, such as quiz, tasks, reminders, and activity log.
        public string DetectIntent(string userInput)
        {
            string input = NormalizeInput(userInput);

            // Quiz commands with different wording.
            if (ContainsAny(input,
                "start quiz", "begin quiz", "play quiz", "quiz me", "open quiz",
                "start mini game", "play game", "cybersecurity game", "test my knowledge"))
            {
                return "start_quiz";
            }

            // Quit quiz commands.
            if (ContainsAny(input,
                "quit quiz", "exit quiz", "stop quiz", "end quiz", "leave quiz", "cancel quiz"))
            {
                return "quit_quiz";
            }

            // Activity log commands.
            if (ContainsAny(input,
                "show activity log", "activity log", "recent actions", "what have you done for me",
                "show history", "chatbot history", "show my activity"))
            {
                return "show_activity_log";
            }

            // Show tasks commands.
            if (ContainsAny(input,
                "show tasks", "view tasks", "task list", "my tasks", "show my tasks",
                "what tasks", "list tasks"))
            {
                return "show_tasks";
            }

            // Add task commands.
            if (ContainsAny(input,
                "add task", "create task", "add a task", "create a task", "task to",
                "i need to", "i must", "i should"))
            {
                return "add_task";
            }

            // Reminder commands.
            if (ContainsAny(input,
                "remind me", "set reminder", "add reminder", "create reminder", "reminder",
                "remind me to", "notify me"))
            {
                return "set_reminder";
            }

            return "chat";
        }

        // Converts user input into a TaskItem object.
        public TaskItem? ParseTaskFromInput(string userInput)
        {
            string input = NormalizeInput(userInput);

            // These phrases indicate where the actual task title starts.
            string[] taskPrefixes =
            {
                "add task", "add a task", "create task", "create a task", "task to",
                "remind me to", "set reminder to", "add reminder to", "i need to", "i must", "i should"
            };

            foreach (string prefix in taskPrefixes)
            {
                int index = input.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);

                if (index >= 0)
                {
                    // Extract the text after the command phrase from the original input.
                    string title = userInput[(index + prefix.Length)..]
                        .Trim(' ', '-', ':', '.', '!');

                    // Remove time words so they do not become part of the task name.
                    title = CleanReminderWords(title);

                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        return new TaskItem
                        {
                            Title = ToTitleCase(title),
                            Description = BuildTaskDescription(title),
                            ReminderDate = ExtractReminderDate(input),
                            IsCompleted = false
                        };
                    }
                }
            }

            return null;
        }

        // Smart NLP simulation for cybersecurity topics.
        // The purpose is to check specific user meanings before the general ResponseBank search.
        private string DetectSmartCybersecurityIntent(string input)
        {
            // 1. Password leaked / stolen intent.
            if (ContainsAny(input,
                "password leaked", "my password got leaked", "leaked password", "password stolen",
                "stolen password", "password hacked", "data breach", "my login was leaked"))
            {
                return BuildSmartResponse("Password Leak",
                    "If your password was leaked, change it immediately. Do not reuse the old password, enable two-factor authentication, and check other accounts where you used the same password.");
            }

            // 2. Strong password intent.
            if (ContainsAny(input,
                "strong password", "make my password stronger", "password stronger", "secure password",
                "better password", "password strength", "weak password", "create a strong password"))
            {
                return BuildSmartResponse("Password Safety",
                    "A strong password should be long, unique, and difficult to guess. Use at least 12 characters, mix uppercase letters, lowercase letters, numbers, and symbols, and avoid names, birthdays, or phone numbers.");
            }

            // 3. Password sharing intent.
            if (ContainsAny(input,
                "share password", "share my password", "send password", "give password",
                "whatsapp password", "email password", "tell someone my password"))
            {
                return BuildSmartResponse("Password Sharing",
                    "Never share your password through WhatsApp, SMS, email, or phone calls. A real company, bank, or support agent should never ask for your password.");
            }

            // 4. Hacked account intent.
            if (ContainsAny(input,
                "account hacked", "my account was hacked", "someone hacked my account",
                "account compromised", "someone logged into my account", "cannot access my account",
                "my account is stolen", "my facebook was hacked", "my instagram was hacked", "my email was hacked"))
            {
                return BuildSmartResponse("Hacked Account",
                    "It sounds like your account may be compromised. Change your password immediately, enable two-factor authentication, log out of all devices, and check your recovery email and phone number.");
            }

            // 5. Suspicious link intent.
            if (ContainsAny(input,
                "clicked a suspicious link", "clicked a bad link", "opened a suspicious link",
                "dangerous link", "suspicious link", "bad link", "unsafe link", "strange link",
                "someone sent me a link", "i clicked a link"))
            {
                return BuildSmartResponse("Suspicious Links",
                    "If you clicked a suspicious link, do not enter any personal details. Close the page, change your password if you typed it in, scan your device, and enable two-factor authentication.");
            }

            // 6. Phishing email intent.
            if (ContainsAny(input,
                "fake email", "phishing email", "scam email", "suspicious email",
                "email asking for password", "email asking for banking details", "email wants my details",
                "verify your account email", "account will be blocked"))
            {
                return BuildSmartResponse("Phishing",
                    "That sounds like a phishing attempt. Do not click links or download attachments. Check the sender address carefully and report the email as phishing if it looks suspicious.");
            }

            // 7. Banking scam and OTP fraud intent.
            if (ContainsAny(input,
                "stole my otp", "someone asked for my otp", "share otp", "bank scam",
                "banking scam", "fake bank message", "fake banking app", "banking details",
                "pin number", "card number", "capitec scam", "fnb scam", "absa scam", "standard bank scam"))
            {
                return BuildSmartResponse("Banking Scam",
                    "Never share your OTP, PIN, card number, or banking password. Banks will not ask for your OTP by phone, SMS, WhatsApp, or email. Contact your bank immediately if you shared it.");
            }

            // 8. SIM swap fraud intent.
            if (ContainsAny(input,
                "sim swap", "sim card hacked", "sim cloned", "lost signal suddenly",
                "phone has no signal", "sim fraud", "my sim stopped working"))
            {
                return BuildSmartResponse("SIM Swap Fraud",
                    "SIM swap fraud happens when criminals move your phone number to another SIM card to receive your OTPs. If your signal disappears unexpectedly, contact your mobile network and bank immediately.");
            }

            // 9. Public Wi-Fi intent.
            if (ContainsAny(input,
                "public wifi", "public wi-fi", "free wifi", "free wi-fi", "airport wifi",
                "coffee shop wifi", "wifi safe", "wi-fi safe", "can wifi hack me",
                "public network", "open wifi"))
            {
                return BuildSmartResponse("Public Wi-Fi",
                    "Public Wi-Fi can be risky because attackers may intercept your information. Avoid banking on public Wi-Fi, do not enter sensitive passwords, and use a trusted VPN where possible.");
            }

            // 10. Malware, virus, spyware intent.
            if (ContainsAny(input,
                "malware", "virus", "computer infected", "phone infected", "laptop acting weird",
                "device acting weird", "spyware", "keylogger", "slow phone", "slow laptop",
                "unknown app", "suspicious download"))
            {
                return BuildSmartResponse("Malware",
                    "Malware is harmful software that can steal data, damage files, or spy on you. Run an antivirus scan, remove suspicious apps, update your system, and avoid downloading unknown files.");
            }

            // 11. Ransomware intent.
            if (ContainsAny(input,
                "ransomware", "files encrypted", "locked files", "pay hackers", "computer locked",
                "data locked", "cannot open files", "my files are locked", "decrypt files"))
            {
                return BuildSmartResponse("Ransomware",
                    "Ransomware locks or encrypts your files and demands payment. Disconnect from the internet, do not rush to pay, report the attack, and restore from a safe backup if available.");
            }

            // 12. Privacy and tracking intent.
            if (ContainsAny(input,
                "apps tracking me", "websites tracking me", "protect my privacy", "online privacy",
                "personal information", "data privacy", "location sharing", "privacy settings",
                "who can see my posts", "stop tracking"))
            {
                return BuildSmartResponse("Privacy",
                    "To protect your privacy, limit app permissions, use strong privacy settings, avoid oversharing personal details, and review what information websites and apps can collect from you.");
            }

            // 13. WhatsApp scam intent.
            if (ContainsAny(input,
                "whatsapp scam", "whatsapp hacked", "verification code", "whatsapp code",
                "someone asked for my whatsapp code", "whatsapp verification", "two step verification whatsapp"))
            {
                return BuildSmartResponse("WhatsApp Security",
                    "WhatsApp scams often involve criminals asking for your verification code. Never share that code. Enable two-step verification in WhatsApp and warn your contacts if your account is compromised.");
            }

            // 14. Online shopping scam intent.
            if (ContainsAny(input,
                "fake shop", "fake store", "online shopping scam", "shopping website",
                "too cheap", "fake website", "online store scam", "delivery scam"))
            {
                return BuildSmartResponse("Online Shopping Scam",
                    "Before buying online, check reviews, website spelling, secure payment options, and contact details. Be careful of deals that look too cheap because fake shops often use unrealistic discounts.");
            }

            // 15. Social engineering intent.
            if (ContainsAny(input,
                "social engineering", "someone pretending", "pretending to be my bank",
                "tricked me", "manipulate", "impersonating", "fake support", "fake technician"))
            {
                return BuildSmartResponse("Social Engineering",
                    "Social engineering is when criminals manipulate people into sharing information or taking unsafe actions. Always verify requests through official channels before trusting messages or calls.");
            }

            // 16. VPN intent.
            if (ContainsAny(input,
                "vpn", "virtual private network", "hide my ip", "secure my connection",
                "should i use vpn", "vpn safe"))
            {
                return BuildSmartResponse("VPN",
                    "A VPN helps protect your connection by encrypting your internet traffic, especially on public Wi-Fi. However, you still need strong passwords, updates, and safe browsing habits.");
            }

            // 17. Software updates intent.
            if (ContainsAny(input,
                "software update", "update my phone", "update my computer", "security update",
                "patch", "system update", "windows update", "phone update"))
            {
                return BuildSmartResponse("Software Updates",
                    "Software updates are important because they fix security weaknesses. Keep your phone, computer, browser, and apps updated to reduce the risk of attacks.");
            }

            // 18. Antivirus intent.
            if (ContainsAny(input,
                "antivirus", "anti virus", "security scan", "scan my device", "virus scan",
                "windows defender", "malware scan"))
            {
                return BuildSmartResponse("Antivirus",
                    "Antivirus software helps detect and remove harmful programs. Keep it updated and run scans if your device becomes slow, shows pop-ups, or behaves strangely.");
            }

            // 19. Cyberbullying intent.
            if (ContainsAny(input,
                "cyberbullying", "online bullying", "someone is harassing me", "online harassment",
                "abusive messages", "threatening messages"))
            {
                return BuildSmartResponse("Cyberbullying",
                    "Cyberbullying should be taken seriously. Save evidence, block the person, report the account, and speak to someone you trust if you feel unsafe.");
            }

            // 20. Identity theft intent.
            if (ContainsAny(input,
                "identity theft", "stole my identity", "using my name", "opened account in my name",
                "personal details stolen", "id number stolen"))
            {
                return BuildSmartResponse("Identity Theft",
                    "Identity theft happens when someone uses your personal information without permission. Report it quickly, monitor your accounts, change passwords, and contact your bank or relevant service providers.");
            }

            // No smart intent was detected, so the normal ResponseBank can continue.
            return string.Empty;
        }

        // Builds a smart response, saves the topic for follow-ups, and applies sentiment tone.
        private string BuildSmartResponse(string topic, string response)
        {
            _currentUser.LastTopic = topic;
            return ApplySentimentPrefix(response, topic);
        }

        // Extracts reminder date from text.
        private DateTime? ExtractReminderDate(string input)
        {
            if (input.Contains("tomorrow"))
            {
                return DateTime.Now.Date.AddDays(1);
            }

            if (input.Contains("today"))
            {
                return DateTime.Now;
            }

            Match match = Regex.Match(input, @"in\s+(\d+)\s+days?");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
            {
                return DateTime.Now.Date.AddDays(days);
            }

            return null;
        }

        // Cleans unnecessary reminder words from task titles.
        private string CleanReminderWords(string title)
        {
            string cleaned = title;

            cleaned = cleaned.Replace("tomorrow", "", StringComparison.OrdinalIgnoreCase)
                             .Replace("today", "", StringComparison.OrdinalIgnoreCase);

            cleaned = Regex.Replace(cleaned, @"\bin\s+\d+\s+days?\b", "", RegexOptions.IgnoreCase);

            return cleaned.Trim(' ', '-', ':', '.', '!');
        }

        // Builds a simple cybersecurity description for tasks.
        private string BuildTaskDescription(string title)
        {
            string lowerTitle = title.ToLower();

            if (lowerTitle.Contains("privacy"))
            {
                return "Review account privacy settings to make sure personal data is protected.";
            }

            if (lowerTitle.Contains("password"))
            {
                return "Update and strengthen passwords to improve account security.";
            }

            if (lowerTitle.Contains("2fa") || lowerTitle.Contains("two-factor") || lowerTitle.Contains("two factor"))
            {
                return "Enable two-factor authentication to add an extra layer of account protection.";
            }

            if (lowerTitle.Contains("antivirus") || lowerTitle.Contains("scan"))
            {
                return "Run a security scan to check for malware or suspicious software.";
            }

            return $"Complete this cybersecurity task: {title}.";
        }

        // Converts text into Title Case safely.
        private string ToTitleCase(string text)
        {
            return string.Join(" ", text
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.Length == 1
                    ? word.ToUpper()
                    : char.ToUpper(word[0]) + word[1..].ToLower()));
        }

        // Normalizes input by converting to lowercase and trimming spaces.
        private string NormalizeInput(string input)
        {
            return input.ToLower().Trim();
        }

        // Checks if input matches any keyword using regex word boundaries.
        // This avoids matching small words inside bigger unrelated words.
        private bool MatchesAnyKeyword(string input, IEnumerable<string> keywords)
        {
            foreach (string keyword in keywords)
            {
                string pattern = $@"\b{Regex.Escape(keyword.ToLower())}\b";

                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // Returns a random response from the response options list.
        private string GetRandomResponse(Response response)
        {
            if (response.ResponseOptions == null || response.ResponseOptions.Count == 0)
            {
                return "I understand the topic, but I do not have a prepared response for it yet.";
            }

            return response.ResponseOptions[_random.Next(response.ResponseOptions.Count)];
        }

        // Detects user sentiment and stores it in memory.
        private void DetectSentiment(string input)
        {
            if (ContainsAny(input, "worried", "scared", "afraid", "nervous", "panic", "anxious"))
            {
                _currentUser.CurrentSentiment = "worried";
            }
            else if (ContainsAny(input, "frustrated", "angry", "annoyed", "upset", "irritated"))
            {
                _currentUser.CurrentSentiment = "frustrated";
            }
            else if (ContainsAny(input, "confused", "do not understand", "don't understand", "lost", "unclear"))
            {
                _currentUser.CurrentSentiment = "confused";
            }
            else if (ContainsAny(input, "curious", "interested", "want to know", "teach me"))
            {
                _currentUser.CurrentSentiment = "curious";
            }
            else
            {
                _currentUser.CurrentSentiment = "neutral";
            }
        }

        // Updates chatbot memory based on user input.
        private void UpdateMemory(string input)
        {
            // Remember favourite topic if the user says they are interested in something.
            if (input.Contains("interested in"))
            {
                string topic = input[(input.IndexOf("interested in", StringComparison.OrdinalIgnoreCase) + "interested in".Length)..].Trim();

                if (!string.IsNullOrWhiteSpace(topic))
                {
                    _currentUser.FavoriteTopic = topic;
                }
            }
            else if (input.Contains("i like"))
            {
                string topic = input[(input.IndexOf("i like", StringComparison.OrdinalIgnoreCase) + "i like".Length)..].Trim();

                if (!string.IsNullOrWhiteSpace(topic))
                {
                    _currentUser.FavoriteTopic = topic;
                }
            }
        }

        // Handles memory questions from the user.
        private string HandleMemoryPrompts(string input)
        {
            if (ContainsAny(input, "what do you remember", "remember about me", "what is my name"))
            {
                string favourite = string.IsNullOrWhiteSpace(_currentUser.FavoriteTopic)
                    ? "You have not told me your favourite cybersecurity topic yet."
                    : $"You are interested in {_currentUser.FavoriteTopic}.";

                return $"Your name is {_currentUser.Name}. {favourite}";
            }

            if (ContainsAny(input, "my favourite topic", "my favorite topic"))
            {
                if (!string.IsNullOrWhiteSpace(_currentUser.FavoriteTopic))
                {
                    return $"I remember that your favourite cybersecurity topic is {_currentUser.FavoriteTopic}.";
                }

                return "I do not know your favourite cybersecurity topic yet. You can say something like: I am interested in privacy.";
            }

            return string.Empty;
        }

        // Handles follow-up questions and continues the last topic.
        private string HandleFollowUp(string input)
        {
            if (!ContainsAny(input,
                "tell me more", "explain more", "another tip", "more", "continue",
                "why", "how", "what do you mean", "give me another example", "example"))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(_currentUser.LastTopic))
            {
                return "I can explain more, but please first ask me about a cybersecurity topic such as phishing, passwords, privacy, malware, or online scams.";
            }

            var previousTopicResponse = _responses.FirstOrDefault(r =>
                string.Equals(r.Category, _currentUser.LastTopic, StringComparison.OrdinalIgnoreCase));

            if (previousTopicResponse != null)
            {
                return ApplySentimentPrefix(
                    $"Here is more about {_currentUser.LastTopic}:\n\n{GetRandomResponse(previousTopicResponse)}",
                    _currentUser.LastTopic);
            }

            // If the topic came from smart intent and does not exist in ResponseBank, give useful topic-based follow-up.
            return ApplySentimentPrefix(GetSmartFollowUpByTopic(_currentUser.LastTopic), _currentUser.LastTopic);
        }

        // Provides follow-up explanations for smart intent topics that may not exist in ResponseBank.
        private string GetSmartFollowUpByTopic(string topic)
        {
            switch (topic.ToLower())
            {
                case "password leak":
                    return "After a password leak, the safest action is to change the password, avoid reusing it, and turn on two-factor authentication. Also watch for unusual login alerts.";

                case "banking scam":
                    return "Banking scams often use fear and urgency. Always verify messages through your bank's official app, branch, or official phone number instead of trusting links in SMS or WhatsApp.";

                case "public wi-fi":
                    return "Public Wi-Fi is risky because attackers may create fake hotspots or intercept traffic. Avoid banking and sensitive logins while connected to open networks.";

                case "ransomware":
                    return "The best defence against ransomware is prevention: keep backups, update software, avoid suspicious attachments, and do not install unknown programs.";

                case "whatsapp security":
                    return "For WhatsApp safety, enable two-step verification, never share verification codes, and be careful when contacts suddenly ask for money or codes.";

                default:
                    return $"Here is more information about {topic}: stay alert, verify suspicious requests, protect your passwords, and avoid sharing sensitive information online.";
            }
        }

        // Adjusts the response based on the user's mood.
        private string ApplySentimentPrefix(string response, string topic)
        {
            if (_currentUser.CurrentSentiment == "worried")
            {
                return "I understand your concern. Cybersecurity can feel stressful, but taking small steps can protect you.\n\n" + response;
            }

            if (_currentUser.CurrentSentiment == "frustrated")
            {
                return "I understand that this can be frustrating. Let me keep it clear and practical.\n\n" + response;
            }

            if (_currentUser.CurrentSentiment == "confused")
            {
                return "No problem, I will explain it in a simple way.\n\n" + response;
            }

            if (_currentUser.CurrentSentiment == "curious")
            {
                return "Great question. Curiosity is a good way to build cybersecurity awareness.\n\n" + response;
            }

            return response;
        }

        // Default fallback response.
        private string GetDefaultResponse()
        {
            return "I am not sure I understand. Try asking about cybersecurity topics such as phishing, passwords, privacy, malware, ransomware, public Wi-Fi, WhatsApp scams, OTP fraud, or online shopping scams.";
        }

        // Helper method to check if input contains any phrase.
        private bool ContainsAny(string input, params string[] phrases)
        {
            return phrases.Any(p => input.Contains(p.ToLower()));
        }
    }
}
