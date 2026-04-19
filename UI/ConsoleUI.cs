using Godot;
using System;

public partial class ConsoleUI : Control
{
    private ConsoleManager _consoleManager;
    private RichTextLabel _logDisplay;
    private LineEdit _inputField;
    private bool _isVisible = false;

    public override void _Ready()
    {
        _consoleManager = new ConsoleManager();

        // 7.2: Quake-style dropdown anchored to the TopWide preset
        SetAnchorsPreset(LayoutPreset.TopWide);
        CustomMinimumSize = new Vector2(0, 300); // Console height
        Visible = _isVisible;

        ColorRect bg = new ColorRect
        {
            Color = new Color(0, 0, 0, 0.9f)
        };
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(vbox);

        _logDisplay = new RichTextLabel
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            ScrollFollowing = true,
            BbcodeEnabled = true
        };
        vbox.AddChild(_logDisplay);

        _inputField = new LineEdit
        {
            PlaceholderText = "Enter command..."
        };
        _inputField.TextSubmitted += OnCommandSubmitted;
        vbox.AddChild(_inputField);

        // Bind directly to the system Logger so we can see all events
        // Note: For a true implementation, Logger should emit an event.
        // For the sake of this code-only structure, we simulate logging here:
        LogMessage("[color=green]Developer Console Initialized. Press '~' to toggle.[/color]");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Toggle Console (Tilde)
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Quoteleft)
        {
            ToggleConsole();
            GetViewport().SetInputAsHandled(); // Prevent the input from leaking to other UI
        }
    }

    private void ToggleConsole()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;

        if (_isVisible)
        {
            _inputField.GrabFocus();
        }
        else
        {
            _inputField.ReleaseFocus();
        }
    }

    private void OnCommandSubmitted(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        LogMessage($"[color=gray]> {text}[/color]");
        _inputField.Clear();

        string result = _consoleManager.ExecuteCommand(text);
        if (!string.IsNullOrEmpty(result))
        {
            LogMessage(result);
        }
    }

    public void LogMessage(string msg)
    {
        _logDisplay.AppendText($"{msg}\n");
    }
}
