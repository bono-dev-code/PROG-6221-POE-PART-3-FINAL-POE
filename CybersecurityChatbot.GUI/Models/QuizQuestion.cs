using System.Collections.Generic;

namespace CybersecurityChatbot.Models
{
    // This class represents a single quiz question in the chatbot.
    // It stores the question, possible answers, the correct answer, and an explanation.
    public class QuizQuestion
    {
        // Stores the actual question text that will be shown to the user.
        public string QuestionText { get; set; } = string.Empty;

        // Stores the list of possible answer options for the question.
        // Example: ["Yes", "No"] or ["A", "B", "C", "D"]
        public List<string> Options { get; set; } = new();

        // Stores the index (position) of the correct answer in the Options list.
        // Example: 0 means the first option is correct.
        public int CorrectOptionIndex { get; set; }

        // Stores an explanation of the correct answer.
        // This helps the user understand why the answer is correct.
        public string Explanation { get; set; } = string.Empty;

        // Indicates whether the question is a True/False type question.
        // True = it is a True/False question
        // False = it is a multiple-choice question
        public bool IsTrueFalse { get; set; }
    }
}