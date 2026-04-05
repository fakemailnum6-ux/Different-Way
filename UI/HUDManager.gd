extends Control

@onready var settings_panel = $SettingsPanel

func _ready():
	settings_panel.hide()

func _on_settings_button_pressed():
	settings_panel.show()

func _on_close_settings_button_pressed():
	settings_panel.hide()
