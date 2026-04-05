extends Node2D

@onready var tavern_menu = $CanvasLayer/TavernMenu
@onready var map_ui = $MapUI
@onready var dialog_box = $CanvasLayer/DialogBox
@onready var quest_log = $CanvasLayer/QuestLog
@onready var hud_manager = $CanvasLayer/HUDManager
@onready var inventory_ui = $CanvasLayer/InventoryUI
@onready var noticeboard_ui = $CanvasLayer/NoticeboardUI
@onready var character_ui = $CanvasLayer/CharacterStatsUI
@onready var combat_ui = $CanvasLayer/CombatUI

var year = 1
var month = 1
var day = 1
var quarter = 0 # 0=Morning, 1=Day, 2=Evening, 3=Night
var gold = 100

var has_quest = false

func _ready():
	tavern_menu.hide()
	dialog_box.hide()
	quest_log.hide()
	inventory_ui.hide()
	noticeboard_ui.hide()
	character_ui.hide()
	combat_ui.hide()

	dialog_box.closed.connect(_on_dialog_box_closed)
	quest_log.closed.connect(_on_quest_log_closed)

	update_time_display()
	update_quest_display()

func update_time_display():
	var time_str = ""
	match quarter:
		0: time_str = "Morning"
		1: time_str = "Day"
		2: time_str = "Evening"
		3: time_str = "Night"
	var full_str = "Year %d, Month %d, Day %d - %s" % [year, month, day, time_str]
	hud_manager.set_time_text(full_str)

func update_gold_display():
	hud_manager.get_node("TopBar/HBoxContainer/GoldLabel").text = "Gold: " + str(gold)

func update_quest_display():
	if has_quest:
		hud_manager.set_quest_text("Active Quest: Slay Rats in the Cellar")
		quest_log.set_quest("Slay 5 Rats in the Cellar")
	else:
		hud_manager.set_quest_text("Active Quest: None")
		quest_log.set_quest("No active quests.")

func advance_time(quarters_to_add: int):
	quarter += quarters_to_add
	while quarter >= 4:
		quarter -= 4
		day += 1
		if day > 30:
			day = 1
			month += 1
			if month > 12:
				month = 1
				year += 1
	update_time_display()

func _on_enter_tavern_button_pressed():
	map_ui.hide()
	tavern_menu.show()

func _on_tavern_menu_dialogue_requested():
	tavern_menu.hide()
	dialog_box.show()

func _on_tavern_menu_cellar_requested():
	tavern_menu.hide()
	combat_ui.show()
	combat_ui.start_combat()

func _on_tavern_menu_noticeboard_requested():
	tavern_menu.hide()
	noticeboard_ui.show()

func _on_tavern_menu_exit_requested():
	tavern_menu.hide()
	map_ui.show()

func _on_hud_quests_requested():
	quest_log.show()

func _on_hud_inventory_requested():
	inventory_ui.show()

func _on_hud_character_requested():
	character_ui.show()

func _on_hud_world_map_requested():
	print("World Map transitions not fully implemented in MVP. Returning to map.")
	map_ui.show()

func _on_hud_skip_time_requested(quarters: int):
	advance_time(quarters)

func _on_inventory_closed():
	inventory_ui.hide()

func _on_noticeboard_closed():
	noticeboard_ui.hide()
	tavern_menu.show()

func _on_noticeboard_quest_accepted():
	has_quest = true
	update_quest_display()
	noticeboard_ui.hide()
	tavern_menu.show()

func _on_dialog_box_closed():
	dialog_box.hide()
	tavern_menu.show()

func _on_dialog_box_buy_drink_requested():
	if gold >= 5:
		gold -= 5
		update_gold_display()
		print("Drink bought!")
	else:
		print("Not enough gold!")

func _on_quest_log_closed():
	quest_log.hide()

func _on_character_closed():
	character_ui.hide()

func _on_combat_closed():
	combat_ui.hide()
	tavern_menu.show()
