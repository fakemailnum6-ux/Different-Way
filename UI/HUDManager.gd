extends Control

signal inventory_requested
signal skip_time_requested(quarters)

@onready var settings_panel = $SettingsPanel
@onready var time_label = $TopBar/HBoxContainer/TimeLabel
@onready var quest_label = $TopBar/HBoxContainer/QuestLabel
@onready var resolution_option = $SettingsPanel/VBoxContainer/HBoxContainerRes/ResolutionOption

const RESOLUTIONS = [
	Vector2i(1920, 1080),
	Vector2i(1600, 900),
	Vector2i(1280, 720),
	Vector2i(1024, 768),
	Vector2i(800, 600)
]

func _ready():
	settings_panel.hide()

	for res in RESOLUTIONS:
		resolution_option.add_item("%d x %d" % [res.x, res.y])

	resolution_option.item_selected.connect(_on_resolution_selected)

func _on_resolution_selected(index: int):
	var size = RESOLUTIONS[index]
	DisplayServer.window_set_size(size)

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
