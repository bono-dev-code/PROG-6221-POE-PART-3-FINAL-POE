using System.IO;
using System.Media;

namespace CybersecurityChatbot.Services
{
    // This service class handles audio playback in the chatbot.
    // It is mainly used to play a greeting sound when the application starts.
    public class AudioService
    {
        // Stores the file path of the audio file (e.g., greeting.wav).
        private readonly string _audioFilePath;

        // Constructor that receives the audio file path.
        public AudioService(string audioFilePath)
        {
            _audioFilePath = audioFilePath;
        }

        // This method plays the greeting audio.
        public void PlayGreeting()
        {
            // Check if the file path is not empty AND the file actually exists
            if (!string.IsNullOrWhiteSpace(_audioFilePath) && File.Exists(_audioFilePath))
            {
                // Create a SoundPlayer object using the audio file
                using SoundPlayer player = new SoundPlayer(_audioFilePath);

                // Load the audio file into memory
                player.Load();

                // Play the audio (non-blocking)
                player.Play();
            }
        }
    }
}