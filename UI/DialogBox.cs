using Godot;
using System;
using System.Threading;

namespace DifferentWay.UI
{
    public partial class DialogBox : Control
    {
        private RichTextLabel _chatHistory;
        private LineEdit _inputField;
        private Button _sendButton;

        public delegate void MessageSentEventHandler(string message);
        public event MessageSentEventHandler OnMessageSent;

        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.BottomWide);
            CustomMinimumSize = new Vector2(0, 300);

            var panel = new Panel();
            panel.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(panel);

            var marginContainer = new MarginContainer();
            marginContainer.SetAnchorsPreset(LayoutPreset.FullRect);
            marginContainer.AddThemeConstantOverride("margin_all", 10);
            panel.AddChild(marginContainer);

            var vbox = new VBoxContainer();
            marginContainer.AddChild(vbox);

            _chatHistory = new RichTextLabel
            {
                SizeFlagsVertical = SizeFlags.ExpandFill,
                ScrollFollowing = true,
                BbcodeEnabled = true
            };
            vbox.AddChild(_chatHistory);

            var hbox = new HBoxContainer();
            vbox.AddChild(hbox);

            _inputField = new LineEdit
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                PlaceholderText = "Введите сообщение..."
            };
            _inputField.TextSubmitted += OnTextSubmitted;
            hbox.AddChild(_inputField);

            _sendButton = new Button { Text = "Отправить" };
            _sendButton.Pressed += () => OnTextSubmitted(_inputField.Text);
            hbox.AddChild(_sendButton);
        }

        private async void OnTextSubmitted(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            AppendMessage("Вы", text, "#AAAAAA");
            _inputField.Clear();

            // Disable input while waiting
            _inputField.Editable = false;
            _sendButton.Disabled = true;

            OnMessageSent?.Invoke(text);

            var contextManager = GetNodeOrNull<DifferentWay.AI.ContextManager>("/root/ContextManager");
            var promptBuilder = GetNodeOrNull<DifferentWay.AI.PromptBuilder>("/root/PromptBuilder");
            var llmClient = GetNodeOrNull<DifferentWay.AI.LLMClient>("/root/LLMClient");
            var graphValidator = GetNodeOrNull<DifferentWay.AI.GraphValidator>("/root/GraphValidator");

            if (contextManager == null || promptBuilder == null || llmClient == null || graphValidator == null)
            {
                AppendMessage("System", "Error: AI Autoloads not found.", "#FF0000");
                EnableInput();
                return;
            }

            string npcId = "npc_1"; // Stub for zero_state_town Tavern Keeper
            string npcName = "Трактирщик Боб";
            string npcGoap = "Ждет клиентов, хочет заработать";

            // 1. Add user message to history
            contextManager.AddMessage(npcId, "user", text);
            var history = contextManager.GetHistory(npcId);

            // 2. Build Prompt
            string systemPrompt = promptBuilder.BuildSystemPrompt();
            string payload = promptBuilder.BuildContextPayload(npcName, npcGoap, "12:00", text, history);

            // 3. Send Request
            string rawResponse = await llmClient.SendRequest(payload, systemPrompt);

            // 4. Validate & Parse
            var aiResponse = graphValidator.ValidateAndParse(rawResponse);

            if (aiResponse != null)
            {
                // Add AI response to history
                contextManager.AddMessage(npcId, "assistant", aiResponse.SpokenText);
                AppendMessage(npcName, aiResponse.SpokenText, "#00FF00");

                // Check triggers
                if (aiResponse.ActionTriggers != null)
                {
                    foreach (var trigger in aiResponse.ActionTriggers)
                    {
                        if (trigger.Type == "end_dialogue")
                        {
                            AppendMessage("System", "Диалог завершен.", "#FFFF00");
                            // E.g. _eventBus.EmitGameStateTransition("Exploration");
                        }
                        else if (trigger.Type == "give_item")
                        {
                            AppendMessage("System", $"Получен предмет: {trigger.Id} x{trigger.Amount}", "#FFFF00");
                        }
                        else if (trigger.Type == "attack")
                        {
                            AppendMessage("System", "Начался бой!", "#FF0000");
                            var combatUI = GetNodeOrNull<CombatUI>("/root/Main/CombatUI");
                            if (combatUI != null)
                            {
                                combatUI.StartCombat(npcName, 100);
                            }
                        }
                    }
                }
            }
            else
            {
                AppendMessage(npcName, "Трактирщик Боб задумчиво молчит, словно забыв слова.", "#FF0000");
            }

            EnableInput();
        }

        public void AppendMessage(string sender, string message, string colorHex = "#FFFFFF")
        {
            _chatHistory.AppendText($"[color={colorHex}][b]{sender}:[/b][/color] {message}\n\n");
        }

        public void EnableInput()
        {
            _inputField.Editable = true;
            _sendButton.Disabled = false;
            _inputField.GrabFocus();
        }
    }
}
