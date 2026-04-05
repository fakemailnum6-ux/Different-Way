extends Control

signal closed

func _on_close_button_pressed():
	closed.emit()
