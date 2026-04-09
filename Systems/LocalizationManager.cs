using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public partial class LocalizationManager : RefCounted
{
    public string CurrentLanguage { get; private set; } = "ru";

    private Dictionary<string, string> _translations = new Dictionary<string, string>();
    private EventBus _eventBus;

    public void Initialize(EventBus eventBus)
    {
        _eventBus = eventBus;
        LoadLanguage(CurrentLanguage);
    }

    public void LoadLanguage(string langCode)
    {
        string path = $"res://Data/Localization/{langCode}.json";

        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"LocalizationManager: Translation file not found at {path}");
            return;
        }

        try
        {
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            string json = file.GetAsText();
            _translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            CurrentLanguage = langCode;

            GD.Print($"LocalizationManager: Loaded language '{langCode}'.");

            // Notify UI elements
            _eventBus?.EmitSignal(EventBus.SignalName.LanguageChanged, langCode);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"LocalizationManager: Error parsing {path}: {ex.Message}");
        }
    }

    public string Translate(string key)
    {
        if (_translations.TryGetValue(key, out string value))
        {
            return value;
        }
        return key; // Fallback to returning the key if missing
    }

    public string Format(string key, Dictionary<string, string> args)
    {
        string text = Translate(key);
        foreach (var arg in args)
        {
            text = text.Replace("{" + arg.Key + "}", arg.Value);
        }
        return text;
    }
}
