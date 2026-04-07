extends Control

@onready var api_key_input = $Panel/VBoxContainer/ApiKeyInput
@onready var save_button = $Panel/VBoxContainer/SaveButton
@onready var close_button = $Panel/VBoxContainer/CloseButton

func _ready():
	save_button.pressed.connect(_on_save_pressed)
	close_button.pressed.connect(hide)

func _on_save_pressed():
	var key = api_key_input.text.strip_edges()
	if key == "":
		return

	var simulation = get_node_or_null("/root/Simulation")
	if simulation:
		# Use Godot's C# interop to call our KeyManager
		# Note: We must call a method on Simulation that wraps the static KeyManager to avoid CS1061 or Object.call limitations.
		simulation.call("SaveApiKey", key)

		# Immediately inject it so it works without a restart
		var live_state = simulation.call("GetLiveState")
		if live_state and live_state.get("LlmClient") != null:
			live_state.get("LlmClient").call("SetCredentials", key, "https://api.openai.com/v1/chat/completions")

	api_key_input.text = ""
	hide()
