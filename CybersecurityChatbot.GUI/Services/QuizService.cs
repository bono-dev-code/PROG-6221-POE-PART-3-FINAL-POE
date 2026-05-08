using System.Collections.Generic;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    // This class is responsible for managing the quiz feature in the chatbot.
    // It controls questions, scoring, progress, and feedback shown to the user.
    public class QuizService
    {
        // A list that stores all quiz questions
        private readonly List<QuizQuestion> _questions;

        // Keeps track of which question the user is currently on
        private int _currentIndex;

        // Stores the user's score 
        public int Score { get; private set; }

        // Indicates if the quiz is currently running or not
        public bool IsQuizActive { get; private set; }

        // Constructor: runs automatically when this class is created
        public QuizService()
        {
            _questions = BuildQuestions(); 
            _currentIndex = 0;             
            Score = 0;                    
            IsQuizActive = false;        
        }

        // This method starts the quiz
        public void StartQuiz()
        {
            _currentIndex = 0;   
            Score = 0;           
            IsQuizActive = true; 
        }

        // This method allows the quiz to be stopped manually
        public void ForceStop()
        {
            IsQuizActive = false; 
        }

        // This method returns the current question object
        public QuizQuestion? GetCurrentQuestion()
        {
            // If quiz is not active OR no more questions left
            if (!IsQuizActive || _currentIndex >= _questions.Count)
            {
                return null; 
            }

            // Return the current question
            return _questions[_currentIndex];
        }

        // This method checks if the selected answer is correct
        // It also returns feedback to the user
        public string SubmitAnswer(int selectedIndex)
        {
            // If quiz is not active, prevent answering
            if (!IsQuizActive)
            {
                return "The quiz is not active right now.";
            }

            // Get the current question
            var currentQuestion = GetCurrentQuestion();

            // If no question is found (safety check)
            if (currentQuestion == null)
            {
                return "There are no more quiz questions.";
            }

            // Check if the selected answer matches the correct answer
            bool correct = selectedIndex == currentQuestion.CorrectOptionIndex;

            // If correct, increase score
            if (correct)
            {
                Score++;
            }

            // Create feedback message for the user
            string feedback = correct
                ? $"Correct! {currentQuestion.Explanation}"
                : $"Incorrect. {currentQuestion.Explanation}";

            // Move to the next question
            _currentIndex++;

            // If all questions are completed
            if (_currentIndex >= _questions.Count)
            {
                IsQuizActive = false; // End quiz

                // Add final score and performance message
                feedback += $"\n\nQuiz complete. Your final score is {Score}/{_questions.Count}. {GetFinalFeedback()}";
            }

            return feedback; 
        }

        // Returns the current question number (for display)
        public int GetQuestionNumber()
        {
            return _currentIndex + 1; 
        }

        // Returns total number of questions in the quiz
        public int GetTotalQuestions()
        {
            return _questions.Count;
        }

        // Provides final feedback based on score
        private string GetFinalFeedback()
        {
            if (Score >= 13)
                return "Excellent work! You are a cybersecurity pro!";
            if (Score >= 10)
                return "Great job! You have strong cybersecurity awareness.";
            if (Score >= 7)
                return "Good effort. Keep learning to stay safer online.";

            // Default feedback for low score
            return "Keep practising. Cybersecurity awareness improves with learning and repetition.";
        }

        // This method builds and returns all quiz questions
       
        private List<QuizQuestion> BuildQuestions()
        {
            return new List<QuizQuestion>
            {
                // Each QuizQuestion object contains:
                // - QuestionText
                // - Options 
                // - CorrectOptionIndex 
                // - Explanation 

                new QuizQuestion
                {
                    QuestionText = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "Reply with your password", "Delete the email", "Report it as phishing", "Ignore it and do nothing" },
                    CorrectOptionIndex = 2,
                    Explanation = "Reporting phishing emails helps prevent scams and protects other users."
                },

                new QuizQuestion
                {
                    QuestionText = "Which password is the strongest?",
                    Options = new List<string> { "123456", "Bono2000", "P@ssword", "Tr#9x!Q7m@Lp2" },
                    CorrectOptionIndex = 3,
                    Explanation = "Strong passwords should be long, unique, and difficult to guess."
                },

                new QuizQuestion
                {
                    QuestionText = "True or False: It is safe to reuse the same password across many websites.",
                    Options = new List<string> { "True", "False" },
                    CorrectOptionIndex = 1,
                    Explanation = "Reusing passwords is risky because one leaked password can expose many accounts.",
                    IsTrueFalse = true 
                },

                new QuizQuestion
                {
                    QuestionText = "What does two-factor authentication help with?",
                    Options = new List<string> { "Making games faster", "Adding an extra security step", "Deleting viruses automatically", "Changing your username" },
                    CorrectOptionIndex = 1,
                    Explanation = "Two-factor authentication adds another layer of protection after your password."
                },

                new QuizQuestion
                {
                    QuestionText = "What is phishing?",
                    Options = new List<string> { "A type of online scam", "A safe website", "A computer brand", "A password manager" },
                    CorrectOptionIndex = 0,
                    Explanation = "Phishing is when criminals try to trick users into revealing sensitive information."
                },

                new QuizQuestion
                {
                    QuestionText = "True or False: HTTPS usually means the connection to a website is more secure.",
                    Options = new List<string> { "True", "False" },
                    CorrectOptionIndex = 0,
                    Explanation = "HTTPS helps protect data sent between your browser and the website.",
                    IsTrueFalse = true
                },

                new QuizQuestion
                {
                    QuestionText = "What should you do before clicking a suspicious link?",
                    Options = new List<string> { "Click it quickly", "Check the sender and link carefully", "Forward it to everyone", "Enter your password first" },
                    CorrectOptionIndex = 1,
                    Explanation = "Checking links and senders helps you avoid phishing and malware attacks."
                },

                new QuizQuestion
                {
                    QuestionText = "What is malware?",
                    Options = new List<string> { "Harmful software", "A secure password", "A computer screen", "A Wi-Fi name" },
                    CorrectOptionIndex = 0,
                    Explanation = "Malware is software designed to harm, steal, or damage data and systems."
                },

                new QuizQuestion
                {
                    QuestionText = "True or False: Public Wi-Fi is always safe for online banking.",
                    Options = new List<string> { "True", "False" },
                    CorrectOptionIndex = 1,
                    Explanation = "Public Wi-Fi can be unsafe, especially for banking or entering sensitive information.",
                    IsTrueFalse = true
                },

                new QuizQuestion
                {
                    QuestionText = "What is social engineering?",
                    Options = new List<string> { "Manipulating people to reveal details", "Building a social media profile", "Installing updates", "Creating a strong password" },
                    CorrectOptionIndex = 0,
                    Explanation = "Social engineering tricks people into giving away information or access."
                },

                new QuizQuestion
                {
                    QuestionText = "What is a good first step to improve privacy on social media?",
                    Options = new List<string> { "Make everything public", "Review privacy settings", "Accept every friend request", "Post personal details" },
                    CorrectOptionIndex = 1,
                    Explanation = "Reviewing privacy settings helps you control who can see your content."
                },

                new QuizQuestion
                {
                    QuestionText = "True or False: Antivirus software should be kept updated.",
                    Options = new List<string> { "True", "False" },
                    CorrectOptionIndex = 0,
                    Explanation = "Updates help antivirus software detect newer threats.",
                    IsTrueFalse = true
                },

                new QuizQuestion
                {
                    QuestionText = "What should you do if a website asks for too much personal information?",
                    Options = new List<string> { "Give everything", "Check if the website is trustworthy", "Use someone else's details", "Ignore all warnings" },
                    CorrectOptionIndex = 1,
                    Explanation = "You should always check whether a website is trustworthy before sharing personal data."
                },

                new QuizQuestion
                {
                    QuestionText = "What is the purpose of a password manager?",
                    Options = new List<string> { "To store and manage passwords safely", "To delete your accounts", "To make Wi-Fi faster", "To remove all security" },
                    CorrectOptionIndex = 0,
                    Explanation = "A password manager helps store strong unique passwords securely."
                },

                new QuizQuestion
                {
                    QuestionText = "True or False: You should download attachments from unknown senders.",
                    Options = new List<string> { "True", "False" },
                    CorrectOptionIndex = 1,
                    Explanation = "Unknown attachments may contain malware or phishing content.",
                    IsTrueFalse = true
                }
            };
        }
    }
}