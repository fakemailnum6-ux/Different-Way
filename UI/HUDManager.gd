extends Control

signal inventory_requested
signal skip_time_requested(quarters)

@onready var settings_panel = $SettingsPanel
@onready var time_label = $TopBar/HBoxContainer/TimeLabel
@onready var quest_label = $TopBar/HBoxContainer/QuestLabel

func _ready():
	settings_panel.hide()

func set_time_text(text: String):
	time_label.text = text

func set_quest_text(text: String):
	quest_label.text = text

func _on_inventory_button_pressed():
	inventory_requested.emit()

func _on_settings_button_pressed():
	settings_panel.show()

func _on_close_settings_button_pressed():
	settings_panel.hide()

func _on_skip_quarter_button_pressed():
	skip_time_requested.emit(1)

func _on_skip_half_button_pressed():
	skip_time_requested.emit(2)

func _on_skip_day_button_pressed():
	skip_time_requested.emit(4)
