extends Node2D

@onready var tavern_menu = $CanvasLayer/TavernMenu
@onready var map_ui = $MapUI

func _ready():
	tavern_menu.hide()

func _on_enter_tavern_button_pressed():
	print("Entering Tavern...")
	map_ui.hide()
	tavern_menu.show()

func _on_tavern_menu_dialogue_requested():
	print("Opening dialogue... (Placeholder)")
	# Here you would instantiate and show the dialogue UI

func _on_tavern_menu_quests_requested():
	print("Opening quests... (Placeholder)")
	# Here you would instantiate and show the quests UI

func _on_tavern_menu_exit_requested():
	print("Exiting Tavern...")
	tavern_menu.hide()
	map_ui.show()
