# Cybersecurity Awareness Chatbot (Part 3 / POE)

**Name:** Nenguda Bono  
**Student ID:** ST10484954  
**Course:** PROG6221 – Programming 2A  
**Institution:** Rosebank College  
**Project:** POE Part 3  

## Project Overview

This project is the final POE version of the Cybersecurity Awareness Chatbot. It builds on Part 1 and Part 2 by keeping the GUI chatbot features and adding advanced functionality required for the POE:

- Task Assistant with reminders
- Database integration using MySQL
- Cybersecurity mini-game quiz
- NLP simulation for flexible commands
- Activity log feature

The application is developed as a WPF desktop GUI and keeps the cybersecurity education focus for South African users.

## Features Implemented

### 1. Chat Assistant
- Voice greeting on launch
- ASCII art in the GUI
- Keyword recognition for cybersecurity topics
- Random responses
- Memory and recall
- Sentiment detection
- Follow-up conversation flow

### 2. Task Assistant with Reminders
- Add cybersecurity tasks with title, description, and optional reminder date
- View tasks in a grid
- Mark tasks as completed
- Delete tasks
- Tasks are stored in MySQL

### 3. Database Integration
- MySQL database support using `MySql.Data`
- Automatic creation of the `Tasks` table
- CRUD operations for tasks

### 4. Cybersecurity Quiz Game
- 12 questions included
- Multiple-choice and true/false style questions
- Immediate feedback after each answer
- Final score and performance message

### 5. NLP Simulation
The chatbot can understand flexible commands such as:
- `Add task to enable 2FA`
- `Remind me to update my password tomorrow`
- `Start quiz`
- `What have you done for me?`
- `Show tasks`

### 6. Activity Log
- Stores recent actions such as:
  - task added
  - reminder set
  - quiz started/completed
  - chatbot interactions
- Shows the last 5–10 actions in the GUI and via chat requests

## Technologies Used
- C#
- .NET 8
- WPF
- MySQL
- MySql.Data package
- GitHub
- GitHub Actions

## Project Structure

```text
CybersecurityChatbot-Part3/
├── CybersecurityChatbot.GUI/
│   ├── Models/
│   ├── Services/
│   ├── Resources/
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── TaskManagerWindow.xaml
│   ├── TaskManagerWindow.xaml.cs
│   ├── App.xaml
│   ├── App.xaml.cs
│   └── CybersecurityChatbot.GUI.csproj
├── Database/
│   └── setup.sql
├── .github/
│   └── workflows/
│       └── dotnet.yml
└── README.md
```

## MySQL Setup

1. Install MySQL Server locally.
2. Run the SQL script in `Database/setup.sql`.
3. Open `Services/DatabaseService.cs`.
4. Update the connection string with your MySQL username and password:
5. Database prompts 
## Use your database
USE cybersecurity_chatbot;
## Make sure the table is correct
DESCRIBE Tasks;
## Viewing task on db
SELECT * FROM ViewTasks;


```csharp
private readonly string connectionString =
            "server=localhost;port=3306;user=root;password=@Bb0760470006;database=cybersecurity_chatbot;";
```

## How to Run the Project

### Requirements
- Windows
- Visual Studio 2022 or later
- .NET 8 SDK
- MySQL Server

### Steps
1. Download The Zipped Folder & Extract it
2. Open the project in Visual Studio.
3. Restore NuGet packages.
4. Configure MySQL as explained above.
5. Build the project.
6. Run the application.

## Example Commands
- `Password Safety`
- `Phishing`
- `I am worried about scams`
- `Tell me more`
- `Add task to enable 2FA`
- `Remind me to update my password tomorrow`
- `Show tasks`
- `Start quiz`
- `What have you done for me?`

## Continuous Integration
This project uses GitHub Actions on Windows to restore and build the WPF project automatically.

 ## Part 3 releases tags
- `v1.0 - Task Assistant Implementation`
- `v1.5 - Database and Quiz Features`
- `v2.0 - Final Cybersecurity Awareness Chatbot POE`
- `v2.1 - Custom Task Manager Window Update`


## License
This project is for educational purposes only.
