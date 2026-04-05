extends Control

# 7.3. GlobalMap: Поддержка Zoom (колесиком). Оснащена выпадающим списком-поиском.
# Выбор открытой деревни в списке мгновенно центрирует камеру.

@export var zoom_speed: float = 0.1
@export var min_zoom: float = 0.5
@export var max_zoom: float = 2.0

@onready var camera = $Camera2D
@onready var search_dropdown = $UISearch/OptionButton
@onready var map_nodes_container = $MapNodes

# Reference to C# WorldTopology
var world_topology_node = null

func _ready():
	var close_btn = get_node_or_null("CloseButton")
	if close_btn: close_btn.pressed.connect(func(): hide())

	_populate_dropdown()
	search_dropdown.item_selected.connect(_on_village_selected)

func _populate_dropdown():
	search_dropdown.clear()
	if world_topology_node == null: return

	# Fetch node data from C# WorldTopology manager
	var unlocked_nodes = world_topology_node.call("GetUnlockedVillages")
	for node in unlocked_nodes:
		search_dropdown.add_item(node.name, node.id)

func _on_village_selected(index: int):
	var village_id = search_dropdown.get_item_id(index)
	# Mгновенно центрирует камеру
	_center_camera_on_node(village_id)

func _center_camera_on_node(village_id: int):
	for map_node in map_nodes_container.get_children():
		if map_node.get("node_id") == village_id:
			camera.global_position = map_node.global_position
			break

func _input(event):
	# Поддержка Zoom (колесиком)
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_WHEEL_UP:
			_zoom_camera(-zoom_speed)
		elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			_zoom_camera(zoom_speed)

func _zoom_camera(delta_zoom: float):
	var current_zoom = camera.zoom.x
	var new_zoom = clamp(current_zoom + delta_zoom, min_zoom, max_zoom)
	camera.zoom = Vector2(new_zoom, new_zoom)
