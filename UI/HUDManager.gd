extends Control

@onready var map_layer = $Map
@onready var location_container = $LocationContainer

var tavern_scene = preload("res://Tavern.tscn")
var current_location: Node = null

func _ready():
	map_layer.location_selected.connect(_on_location_selected)

func _on_location_selected(location_name: String):
	map_layer.hide()

	if current_location != null:
		current_location.queue_free()

	if location_name == "Tavern":
		current_location = tavern_scene.instantiate()
		location_container.add_child(current_location)
		if current_location.has_signal("return_to_map"):
			current_location.return_to_map.connect(_on_return_to_map)

func _on_return_to_map():
	if current_location != null:
		current_location.queue_free()
		current_location = null
	map_layer.show()
