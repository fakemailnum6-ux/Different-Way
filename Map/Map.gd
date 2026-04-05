extends Node2D

@onready var tavern_menu = $CanvasLayer/TavernMenu
@onready var map_ui = $MapUI
@onready var dialog_box = $CanvasLayer/DialogBox
@onready var quest_log = $CanvasLayer/QuestLog

func _ready():
	tavern_menu.hide()
	dialog_box.hide()
	quest_log.hide()

	dialog_box.closed.connect(_on_dialog_box_closed)
	quest_log.closed.connect(_on_quest_log_closed)

func _on_enter_tavern_button_pressed():
	map_ui.hide()
	tavern_menu.show()

func _on_tavern_menu_dialogue_requested():
	tavern_menu.hide()
	dialog_box.show()

func _on_tavern_menu_quests_requested():
	tavern_menu.hide()
	quest_log.show()

func _on_tavern_menu_exit_requested():
	tavern_menu.hide()
	map_ui.show()

func _on_dialog_box_closed():
	dialog_box.hide()
	tavern_menu.show()

func _on_quest_log_closed():
	quest_log.hide()
	tavern_menu.show()
