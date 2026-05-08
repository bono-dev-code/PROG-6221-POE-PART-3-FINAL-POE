using System.Collections.Generic;

namespace CybersecurityChatbot.Models
{
    // This class represents a chatbot response.
    // It connects user keywords to possible chatbot replies.
    public class Response
    {
        // Stores the keywords that trigger this response.
        // Example: "hello", "hi", "hey"
        public string[] Keywords { get; set; }

        // Stores multiple possible responses for variation.
        // The chatbot can randomly pick one to sound more natural.
        public List<string> ResponseOptions { get; set; }

        // Stores the category of the response (e.g., Greeting, Help, Password Safety).
        public string Category { get; set; }

        // Constructor used to create a new Response object.
        // It assigns keywords, response options, and category.
        public Response(string[] keywords, List<string> responseOptions, string category = "General")
        {
            Keywords = keywords;
            ResponseOptions = responseOptions;
            Category = category;
        }
    }

    // This static class acts as a storage (bank) of all chatbot responses.
    // It returns a list of predefined responses used by the chatbot.
    public static class ResponseBank
    {
        // This method returns all chatbot responses as a list.
        public static List<Response> GetResponses()
        {
            return new List<Response>
            {
                // Greeting responses
                new Response(new[] { "hello", "hi", "hey", "greetings" },
                    new List<string>
                    {
                        "Hello! Welcome to the Cybersecurity Awareness Bot. I am ready to help you stay safe online today.",
                        "Hi there! I am your Cybersecurity Awareness Assistant. Ask me anything about online safety.",
                        "Hey! I am here to guide you through important cybersecurity tips and safe online habits."
                    }, "Greeting"),

                // Purpose of the chatbot
                new Response(new[] { "purpose", "what can you do", "what do you do", "your purpose", "what is your purpose" },
                    new List<string>
                    {
                        "I am your Cybersecurity Awareness Assistant. I can help with passwords, phishing, malware, privacy, suspicious links, online scams, 2FA, tasks, reminders, and a cybersecurity quiz.",
                        "My purpose is to educate users about cybersecurity threats and help them build safe online habits in a simple and practical way."
                    }, "Purpose"),

                // Help / topics user can ask about
                new Response(new[] { "help", "topics", "what can i ask you about", "what else" },
                    new List<string>
                    {
                        "You can ask me about passwords, phishing, privacy, safe browsing, malware, suspicious links, identity theft, two-factor authentication, tasks, reminders, quiz, and activity log.",
                        "I can help with cybersecurity tips, create tasks, set reminders, start a quiz, and show your recent activity log."
                    }, "Help"),

                // Password safety responses
                new Response(new[] { "password", "passwords", "strong password", "secure password", "password safety" },
                    new List<string>
                    {
                        "PASSWORD SAFETY TIPS:\n1. Use at least 12 characters.\n2. Mix uppercase letters, lowercase letters, numbers, and symbols.\n3. Do not reuse passwords across accounts.\n4. Avoid personal details like your name or birthday.\n5. Use a password manager where possible.",
                        "A strong password should be long, unique, and difficult to guess. Avoid names, dates of birth, phone numbers, and common words. A password manager can help you create and store strong passwords safely."
                    }, "Password Safety"),

                // Phishing awareness responses
                new Response(new[] { "phishing", "scam", "scams", "phishing scams", "fake email" },
                    new List<string>
                    {
                        "PHISHING AWARENESS:\nPhishing is when criminals pretend to be trusted people or organisations to trick you into giving away sensitive information. Warning signs include urgent language, suspicious links, strange sender addresses, and requests for passwords or banking details.",
                        "If a message pressures you to click quickly, confirm account details, or send personal information, it may be phishing. Stop, verify the sender, and never rush."
                    }, "Phishing"),

                // Privacy responses
                new Response(new[] { "privacy", "online privacy", "protect my privacy" },
                    new List<string>
                    {
                        "ONLINE PRIVACY TIPS:\nReview privacy settings on your accounts, share less personal information publicly, use strong passwords, and enable two-factor authentication.",
                        "Protecting your privacy means controlling what you share, who can see it, and how your accounts are secured."
                    }, "Privacy"),

                // Malware responses
                new Response(new[] { "malware", "virus", "trojan", "ransomware", "worm" },
                    new List<string>
                    {
                        "MALWARE PROTECTION:\nMalware is harmful software that can steal data, damage files, or spy on you. Keep antivirus software updated, avoid suspicious downloads, and never open unknown attachments.",
                        "Signs of malware can include a slow computer, strange pop-ups, missing files, or programs opening on their own. Regular updates and backups help protect you."
                    }, "Malware"),

                // Two-factor authentication responses
                new Response(new[] { "2fa", "two factor authentication", "two-factor authentication", "mfa", "multi factor authentication", "multi-factor authentication" },
                    new List<string>
                    {
                        "TWO-FACTOR AUTHENTICATION:\nTwo-factor authentication adds an extra layer of security. Even if someone steals your password, they still need a second verification step to log in.",
                        "Using 2FA is one of the best ways to protect your accounts. Enable it on email, banking, social media, and shopping accounts."
                    }, "Authentication"),

                // Safe browsing responses
                new Response(new[] { "safe browsing", "browsing", "internet safety", "online safety" },
                    new List<string>
                    {
                        "SAFE BROWSING TIPS:\nCheck for HTTPS, avoid suspicious downloads, keep your browser updated, and be careful with pop-ups and unknown websites.",
                        "Safe browsing means thinking before you click. Always check website legitimacy and avoid sensitive activity on public Wi-Fi."
                    }, "Safe Browsing"),

                // Identity theft responses
                new Response(new[] { "identity theft", "identity", "stolen identity" },
                    new List<string>
                    {
                        "Identity theft happens when criminals steal your personal information and use it fraudulently. Protect your ID number, passwords, banking details, and one-time pins.",
                        "To reduce identity theft risk, do not overshare online, use strong passwords, and be careful with suspicious calls, forms, and messages."
                    }, "Identity Theft"),

                // Suspicious links responses
                new Response(new[] { "link", "links", "suspicious link", "unsafe link" },
                    new List<string>
                    {
                        "Before clicking a link, ask yourself whether you trust the sender, whether the address looks correct, and whether the message is trying to scare or rush you.",
                        "Hover over a link before clicking it. If the real destination looks strange or unrelated to the message, it may be unsafe."
                    }, "Suspicious Links"),

                // Exit / goodbye responses
                new Response(new[] { "bye", "goodbye", "exit", "quit", "thanks", "thank you" },
                    new List<string>
                    {
                        "Thank you for using the Cybersecurity Awareness Bot. Stay safe online, {userName}!",
                        "Goodbye, {userName}. Remember to think before you click and protect your information online."
                    }, "Exit")
            };
        }
    }
}