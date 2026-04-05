extends Control

signal closed
signal quest_accepted

func _on_accept_quest_button_pressed():
	quest_accepted.emit()

func _on_close_button_pressed():
	closed.emit()
