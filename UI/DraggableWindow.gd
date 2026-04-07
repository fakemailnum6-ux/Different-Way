extends Control

var _dragging = false

func _ready():
	# Make sure the panel can receive mouse events
	mouse_filter = Control.MOUSE_FILTER_PASS
	gui_input.connect(_on_gui_input)

func _on_gui_input(event: InputEvent):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			_dragging = event.pressed
			if _dragging:
				# Bring parent (the Window root) to the front
				var window_root = get_parent()
				var layer = window_root.get_parent()
				layer.move_child(window_root, layer.get_child_count() - 1)

	elif event is InputEventMouseMotion and _dragging:
		var window_root = get_parent()
		window_root.global_position += event.relative
