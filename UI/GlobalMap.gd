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

	# Fetch topology from GameState
	var simulation = get_node_or_null("/root/Simulation")
	if simulation:
		var live_state = simulation.call("GetLiveState")
		if live_state:
			world_topology_node = live_state.call("GetTopology")

	_populate_dropdown()
	_draw_map()
	search_dropdown.item_selected.connect(_on_village_selected)

func _populate_dropdown():
	search_dropdown.clear()
	if world_topology_node == null: return

	# Fetch node data from C# WorldTopology manager
	var unlocked_nodes = world_topology_node.call("GetUnlockedVillages")
	var idx = 0
	for node in unlocked_nodes:
		search_dropdown.add_item(node.get("name"), idx)
		search_dropdown.set_item_metadata(idx, node.get("id"))
		idx += 1

func _draw_map():
	if world_topology_node == null: return

	# Clear canvas
	for child in map_nodes_container.get_children():
		child.queue_free()

	# Draw routes
	var routes = world_topology_node.call("GetAllRoutes")
	var all_nodes = world_topology_node.call("GetAllNodes")

	for route in routes:
		var start_id = route.get("StartNodeId")
		var end_id = route.get("EndNodeId")
		var s_node = _find_node(all_nodes, start_id)
		var e_node = _find_node(all_nodes, end_id)

		if s_node and e_node:
			var line = Line2D.new()
			line.add_point(Vector2(s_node.get("X"), s_node.get("Y")))
			line.add_point(Vector2(e_node.get("X"), e_node.get("Y")))
			line.width = 4.0
			line.default_color = Color(0.5, 0.4, 0.3, 0.8) # Road color
			map_nodes_container.add_child(line)

	# Draw nodes
	for node in all_nodes:
		var btn = Button.new()
		var n_name = node.get("name")
		var is_unlocked = node.get("IsUnlocked")

		btn.text = n_name if is_unlocked else "???"
		btn.custom_minimum_size = Vector2(100, 40)
		# Center button on coordinate
		btn.position = Vector2(node.get("X") - 50, node.get("Y") - 20)
		btn.set("node_id", node.get("id")) # Custom dynamic property for camera centering

		if not is_unlocked:
			btn.disabled = true

		map_nodes_container.add_child(btn)

func _find_node(node_list: Array, id: String):
	for n in node_list:
		if n.get("id") == id:
			return n
	return null

func _on_village_selected(index: int):
	var village_id = search_dropdown.get_item_metadata(index)
	# Mгновенно центрирует камеру
	_center_camera_on_node(village_id)

func _center_camera_on_node(village_id: String):
	for map_node in map_nodes_container.get_children():
		if map_node is Button and map_node.get("node_id") == village_id:
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
