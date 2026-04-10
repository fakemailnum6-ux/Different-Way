using Godot;
using System;

public partial class DialogBox : Control
{
    private VBoxContainer _mainContainer;
    private ScrollContainer _scrollContainer;
    private RichTextLabel _chatHistory;

    private HBoxContainer _inputPanel;
    private LineEdit _userInput;
    private Button _sendButton;

    public override void _Ready()
    {
        // 7.3: Absolute positioning forbidden, use Anchors
        SetAnchorsPreset(LayoutPreset.FullRect);

        // Semi-transparent background
        ColorRect bg = new ColorRect
        {
            Color = new Color(0.1f, 0.1f, 0.1f, 0.8f)
        };
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        // Main layout container
        MarginContainer margin = new MarginContainer();
        margin.SetAnchorsPreset(LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_top", 20);
        margin.AddThemeConstantOverride("margin_bottom", 20);
        AddChild(margin);

        _mainContainer = new VBoxContainer();
        margin.AddChild(_mainContainer);

        // Chat History Scroll
        _scrollContainer = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        _mainContainer.AddChild(_scrollContainer);

        _chatHistory = new RichTextLabel
        {
            AutowrapMode = TextServer.AutowrapMode.Word,
            ScrollActive = false,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            BbcodeEnabled = true,
            FitContent = true
        };
        _scrollContainer.AddChild(_chatHistory);

        // Input Panel (Text field + Button)
        _inputPanel = new HBoxContainer();
        _mainContainer.AddChild(_inputPanel);

        _userInput = new LineEdit
        {
            PlaceholderText = "Respond...",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            CaretBlink = true
        };
        _userInput.TextSubmitted += OnTextSubmitted;
        _inputPanel.AddChild(_userInput);

        _sendButton = new Button
        {
            Text = "Send"
        };
        _sendButton.Pressed += OnSendPressed;
        _inputPanel.AddChild(_sendButton);

        // Ensure history auto-scrolls
        _chatHistory.Resized += () =>
        {
            var bar = _scrollContainer.GetVScrollBar();
            bar.Value = bar.MaxValue;
        };

        // Demo text
        AppendMessage("System", "[color=yellow]Connected to Neural Network.[/color]");
    }

    private void OnTextSubmitted(string newText)
    {
        ProcessInput(newText);
    }

    private void OnSendPressed()
    {
        ProcessInput(_userInput.Text);
    }

    private void ProcessInput(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        AppendMessage("You", text);
        _userInput.Clear();

        // Pass intent to simulation command queue
        // ServiceLocator.Simulation.QueueCommand(new RequestAiDialogueCommand(text));
    }

    public void AppendMessage(string sender, string message)
    {
        string color = sender == "You" ? "lightblue" : "white";
        _chatHistory.AppendText($"[color={color}][b]{sender}:[/b][/color] {message}\n\n");
    }

    public void ClearChat()
    {
        _chatHistory.Clear();
    }
}
