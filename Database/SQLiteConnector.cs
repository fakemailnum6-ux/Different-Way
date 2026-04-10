using Godot;
using SQLite;
using System.IO;

public partial class SQLiteConnector : RefCounted
{
    private SQLiteConnection _db;

    private const string SaveDir = "user://saves/";
    private const string CurrentSaveFile = "autosave.db";

    public void Connect()
    {
        string dirPath = ProjectSettings.GlobalizePath(SaveDir);
        string dbPath = Path.Combine(dirPath, CurrentSaveFile);

        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
            ServiceLocator.Logger.LogInfo("SQLiteConnector: Created saves directory.");
        }

        _db = new SQLiteConnection(dbPath);
        ServiceLocator.Logger.LogInfo($"SQLiteConnector: Connected to DB at {dbPath}");

        InitializeTables();
    }

    private void InitializeTables()
    {
        // Creates tables if they don't exist
        _db.CreateTable<PlayerStateDto>();
        _db.CreateTable<InventoryItemDto>();
        _db.CreateTable<WorldNodeDto>();
        _db.CreateTable<EntityStateDto>();
        _db.CreateTable<QuestProgressDto>();
        _db.CreateTable<DialogueCacheDto>();

        ServiceLocator.Logger.LogInfo("SQLiteConnector: Tables initialized.");
    }

    public SQLiteConnection GetConnection()
    {
        return _db;
    }

    public void Close()
    {
        _db?.Close();
    }
}
