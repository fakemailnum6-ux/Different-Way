using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace DifferentWay.Database;

public class MemoryManager
{
    private readonly SQLiteConnector _dbConnector;

    public MemoryManager(SQLiteConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    public void SaveMemory(string keywords, string content)
    {
        if (_dbConnector == null) return;

        try
        {
            using var conn = _dbConnector.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Memories (timestamp, keywords, content) VALUES (@time, @keys, @content)";
            cmd.Parameters.AddWithValue("@time", DateTime.UtcNow.ToString("O"));
            cmd.Parameters.AddWithValue("@keys", keywords.ToLowerInvariant());
            cmd.Parameters.AddWithValue("@content", content);
            cmd.ExecuteNonQuery();

            DifferentWay.Core.GameLogger.Log($"Воспоминание сохранено: {content}");
        }
        catch (Exception ex)
        {
            DifferentWay.Core.GameLogger.LogError($"Failed to save memory: {ex.Message}");
        }
    }

    public List<string> FetchRelevantMemories(string promptText, int limit = 3)
    {
        var results = new List<string>();
        if (_dbConnector == null || string.IsNullOrWhiteSpace(promptText)) return results;

        // Naive split of prompt into words for basic keyword matching (Stub for real VectorDB)
        var words = promptText.ToLowerInvariant().Split(new[] { ' ', '.', ',', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0) return results;

        try
        {
            using var conn = _dbConnector.GetConnection();
            using var cmd = conn.CreateCommand();

            // Construct a basic LIKE query for the keywords
            string query = "SELECT content FROM Memories WHERE ";
            var conditions = new List<string>();

            for (int i = 0; i < words.Length; i++)
            {
                // Only search words longer than 2 chars to allow 3-letter RPG words (меч, лук, орк)
                if (words[i].Length > 2)
                {
                    string paramName = $"@word{i}";
                    conditions.Add($"(keywords LIKE {paramName} OR content LIKE {paramName})");
                    cmd.Parameters.AddWithValue(paramName, $"%{words[i]}%");
                }
            }

            if (conditions.Count == 0) return results; // No significant words to search

            query += string.Join(" OR ", conditions);
            query += " ORDER BY id DESC LIMIT @limit";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@limit", limit);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(reader.GetString(0));
            }
        }
        catch (Exception ex)
        {
            DifferentWay.Core.GameLogger.LogError($"Failed to fetch memories: {ex.Message}");
        }

        return results;
    }
}
