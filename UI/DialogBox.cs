using Godot;
using System;
using DifferentWay.Core;
using DifferentWay.AI;

public partial class DialogBox : Control
{
	private Label _npcText;
	private LineEdit _playerInput;
	private Button _sendButton;

	private LLMClient _llmClient = new LLMClient();
	private PromptBuilder _promptBuilder = new PromptBuilder();
	private NPC _currentNpc;

	public override void _Ready()
	{
		_npcText = GetNode<Label>("VBoxContainer/NpcText");
		_playerInput = GetNode<LineEdit>("VBoxContainer/HBoxContainer/PlayerInput");
		_sendButton = GetNode<Button>("VBoxContainer/HBoxContainer/SendButton");

		_sendButton.Pressed += OnSendButtonPressed;

		// Initial setup for MVP
		GameManager.Instance.StartMVP();
		_currentNpc = GameManager.Instance.Simulation.Npcs[0];
		_npcText.Text = $"You see {_currentNpc.Name}. {_currentNpc.Description}";
	}

	private async void OnSendButtonPressed()
	{
		string input = _playerInput.Text;
		if (string.IsNullOrEmpty(input)) return;

		_playerInput.Text = "";
		_npcText.Text = "Barnaby is thinking...";

		string prompt = _promptBuilder.BuildNpcPrompt(_currentNpc, input);
		string response = await _llmClient.GetResponseAsync(prompt);

		_npcText.Text = response;

		// Complete the quest if it's the first time talking
		GameManager.Instance.Simulation.CompleteQuest("quest_talk_to_innkeeper");
	}
}
