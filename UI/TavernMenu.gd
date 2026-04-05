extends Control

signal dialogue_requested
signal noticeboard_requested
signal cellar_requested
signal exit_requested

func _on_dialogue_button_pressed():
	print("Dialogue button pressed")
	dialogue_requested.emit()

func _on_noticeboard_button_pressed():
	print("Noticeboard button pressed")
	noticeboard_requested.emit()

func _on_cellar_button_pressed():
	print("Cellar button pressed")
	cellar_requested.emit()

func _on_exit_button_pressed():
	print("Exit button pressed")
	exit_requested.emit()
