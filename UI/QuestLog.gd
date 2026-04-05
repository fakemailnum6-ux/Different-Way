extends Control

signal closed

@onready var quest_label = $Panel/MarginContainer/VBoxContainer/QuestLabel

func set_quest(text: String):
	if quest_label:
		quest_label.text = text

func _on_close_button_pressed():
	closed.emit()
