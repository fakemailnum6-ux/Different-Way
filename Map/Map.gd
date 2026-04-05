extends Node2D

@onready var dialog_box = $UILayer/DialogBox
@onready var char_sheet = $UILayer/CharSheetUI
@onready var inventory_ui = $UILayer/InventoryUI
@onready var crafting_ui = $UILayer/CraftingUI
@onready var global_map = $UILayer/GlobalMap

@onready var active_quests_label = $UILayer/ActiveQuestsPanel/VBoxContainer/QuestsLabel

func _ready():
	_hide_all_windows()

	$UILayer/Locations/PlazaButton.pressed.connect(func(): _interact_npc("Староста"))
	$UILayer/Locations/TavernButton.pressed.connect(func(): _interact_npc("Алхимик"))
	$UILayer/Locations/SmithyButton.pressed.connect(func(): _interact_npc("Кузнец"))
	$UILayer/Locations/ForestButton.pressed.connect(func(): _interact_npc("Капитан"))

	$UILayer/HUD/CharButton.pressed.connect(func(): _toggle_window(char_sheet))
	$UILayer/HUD/InvButton.pressed.connect(func(): _toggle_window(inventory_ui))
	$UILayer/HUD/CraftButton.pressed.connect(func(): _toggle_window(crafting_ui))
	$UILayer/HUD/MapButton.pressed.connect(func(): _toggle_window(global_map))

	# Connect to Quest accepted signal
	dialog_box.quest_accepted.connect(_on_quest_accepted)

func _hide_all_windows():
	dialog_box.hide()
	char_sheet.hide()
	inventory_ui.hide()
	crafting_ui.hide()
	global_map.hide()

func _toggle_window(window: Control):
	var is_visible = window.visible
	_hide_all_windows()
	window.visible = !is_visible

func _interact_npc(npc_name: String):
	_hide_all_windows()
	dialog_box.show()

	var welcome_text = ""
	var quest_id = ""
	match npc_name:
		"Староста": welcome_text = "Староста: Добро пожаловать в Дубовую Гавань. Волки снова воют на опушке... (Квест: Убедить старосту)"
		"Кузнец": welcome_text = "Кузнец: Оружие затупилось? Неси материалы. (Квест: Принести травы)"
		"Алхимик": welcome_text = "Алхимик: Мои зелья вернут тебя с того света... (Respawn точка)"
		"Капитан": welcome_text = "Капитан: Готов к бою? (Квест: Убить волков)"

	dialog_box.get_node("ScrollContainer/ChatHistory").text = ""
	dialog_box.call("_append_chat", welcome_text)

func _on_quest_accepted(quest_text: String):
	active_quests_label.text += "\n- " + quest_text
