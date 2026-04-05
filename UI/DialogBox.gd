extends Control

signal closed
signal buy_drink_requested

@onready var npc_text = $Panel/MarginContainer/VBoxContainer/NPCText

func show_dialogue(text: String):
	if npc_text:
		npc_text.text = text

func _on_buy_drink_button_pressed():
	buy_drink_requested.emit()

func _on_close_button_pressed():
	closed.emit()
