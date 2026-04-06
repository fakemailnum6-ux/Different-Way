extends Control

@onready var log_text = $VBoxContainer/LogText
@onready var close_button = $VBoxContainer/CloseButton

func _ready():
	# Connect to the C# GameLogger singleton
	var game_logger = get_node_or_null("/root/GameLogger")
	if game_logger:
		game_logger.LogMessage.connect(_on_log_message)
	else:
		printerr("GameLogger not found in Autoloads!")

	close_button.pressed.connect(func(): hide())

func _on_log_message(message: String):
	log_text.append_text(message + "\n")
