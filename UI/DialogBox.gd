extends Control

signal closed

@onready var npc_text = $Panel/MarginContainer/VBoxContainer/NPCText

func show_dialogue(text: String):
	if npc_text:
		npc_text.text = text

func _on_close_button_pressed():
	closed.emit()
