-- This line creates the database if it does not already exist.
CREATE DATABASE IF NOT EXISTS cybersecurity_chatbot;

-- This line tells MySQL to use the created database.
USE cybersecurity_chatbot;

-- This creates the Tasks table if it does not already exist.
CREATE TABLE IF NOT EXISTS Tasks (
    
    -- Unique ID for each task (automatically increases)
    Id INT AUTO_INCREMENT PRIMARY KEY,
    
    -- Stores the title of the task (short name)
    Title VARCHAR(150) NOT NULL,
    
    -- Stores a detailed description of the task
    Description VARCHAR(500) NOT NULL,
    
    -- Stores the reminder date (can be empty if no reminder is set)
    ReminderDate DATETIME NULL,
    
    -- Stores whether the task is completed or not
    -- 0 = Not completed, 1 = Completed
    IsCompleted BIT NOT NULL DEFAULT 0
);