extends Control

signal dialogue_requested
signal quests_requested
signal exit_requested

func _on_dialogue_button_pressed():
	print("Dialogue button pressed")
	dialogue_requested.emit()

func _on_quests_button_pressed():
	print("Quests button pressed")
	quests_requested.emit()

func _on_exit_button_pressed():
	print("Exit button pressed")
	exit_requested.emit()
