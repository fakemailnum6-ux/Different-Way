extends Control

signal location_selected(location_name: String)

@onready var tavern_button = $TavernButton

func _ready():
	tavern_button.pressed.connect(_on_tavern_pressed)

func _on_tavern_pressed():
	location_selected.emit("Tavern")
