using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    // This service handles all MySQL database work for the Task Assistant.
    // It allows the chatbot to create the database, create the Tasks table,
    // add tasks, view tasks, update reminders, mark tasks complete, and delete tasks.
    public class DatabaseService
    {
        // This connects to the MySQL server only.
        // It is used to create the database if it does not already exist.
        private readonly string serverConnectionString =
            "server=localhost;port=3306;user=root;password=@Bb0760470006;";

        // This connects directly to the cybersecurity_chatbot database.
        private readonly string connectionString =
            "server=localhost;port=3306;user=root;password=@Bb0760470006;database=cybersecurity_chatbot;";

        // This method makes sure the database and Tasks table exist.
        public bool EnsureDatabaseSetup()
        {
            try
            {
                using (var conn = new MySqlConnection(serverConnectionString))
                {
                    conn.Open();

                    string createDatabaseQuery =
                        "CREATE DATABASE IF NOT EXISTS cybersecurity_chatbot;";

                    using var cmd = new MySqlCommand(createDatabaseQuery, conn);
                    cmd.ExecuteNonQuery();
                }

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // TINYINT is easier and safer for C# than BIT.
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS Tasks (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            Title VARCHAR(255) NOT NULL,
                            Description TEXT NOT NULL,
                            ReminderDate DATETIME NULL,
                            IsCompleted TINYINT(1) NOT NULL DEFAULT 0
                        );";

                    using var cmd = new MySqlCommand(createTableQuery, conn);
                    cmd.ExecuteNonQuery();

                    // This fixes older tables that were created with BIT instead of TINYINT.
                    string fixColumnQuery = @"
                        ALTER TABLE Tasks 
                        MODIFY COLUMN IsCompleted TINYINT(1) NOT NULL DEFAULT 0;";

                    using var fixCmd = new MySqlCommand(fixColumnQuery, conn);
                    fixCmd.ExecuteNonQuery();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        // This method adds a new cybersecurity task to the database.
        public int AddTask(TaskItem task)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                INSERT INTO Tasks (Title, Description, ReminderDate, IsCompleted)
                VALUES (@title, @description, @reminderDate, @isCompleted);
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@title", task.Title);
            cmd.Parameters.AddWithValue("@description", task.Description);

            if (task.ReminderDate.HasValue)
                cmd.Parameters.AddWithValue("@reminderDate", task.ReminderDate.Value);
            else
                cmd.Parameters.AddWithValue("@reminderDate", DBNull.Value);

            cmd.Parameters.AddWithValue("@isCompleted", task.IsCompleted ? 1 : 0);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // This method gets all tasks from the database and sends them back to the GUI.
        public List<TaskItem> GetTasks()
        {
            List<TaskItem> tasks = new();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                SELECT Id, Title, Description, ReminderDate, IsCompleted
                FROM Tasks
                ORDER BY Id DESC;";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                DateTime? reminderDate = null;

                if (reader["ReminderDate"] != DBNull.Value)
                {
                    reminderDate = Convert.ToDateTime(reader["ReminderDate"]);
                }

                bool isCompleted = false;

                if (reader["IsCompleted"] != DBNull.Value)
                {
                    isCompleted = Convert.ToInt32(reader["IsCompleted"]) == 1;
                }

                tasks.Add(new TaskItem
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Title = reader["Title"]?.ToString() ?? "",
                    Description = reader["Description"]?.ToString() ?? "",
                    ReminderDate = reminderDate,
                    IsCompleted = isCompleted
                });
            }

            return tasks;
        }

        // This method updates the reminder date for a saved task.
        public void UpdateTaskReminder(int taskId, DateTime reminderDate)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                UPDATE Tasks
                SET ReminderDate = @reminderDate
                WHERE Id = @id;";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@reminderDate", reminderDate);
            cmd.Parameters.AddWithValue("@id", taskId);

            cmd.ExecuteNonQuery();
        }

        // This method marks a task as complete in the database.
        public void MarkTaskCompleted(int taskId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                UPDATE Tasks
                SET IsCompleted = 1
                WHERE Id = @id;";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", taskId);
            cmd.ExecuteNonQuery();
        }

        // This method deletes a task from the database using the task ID.
        public void DeleteTask(int taskId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                DELETE FROM Tasks
                WHERE Id = @id;";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", taskId);
            cmd.ExecuteNonQuery();
        }
    }
}