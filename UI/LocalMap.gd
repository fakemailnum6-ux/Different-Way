extends Control

# 7.3. LocalMap: Узловая структура комнаты/подземелья.

@onready var room_container = $RoomContainer

func render_dungeon(rooms_data):
	# Clear previous rooms
	for child in room_container.get_children():
		child.queue_free()

	# Instantiates node structure of rooms based on dungeon map graph
	for room in rooms_data:
		var room_node = preload("res://UI/LocalMapRoom.tscn").instantiate()
		room_node.position = room.position
		room_container.add_child(room_node)

		if room.is_cleared:
			room_node.modulate = Color(0, 1, 0) # Green for cleared
		else:
			room_node.modulate = Color(1, 0, 0) # Red for active
