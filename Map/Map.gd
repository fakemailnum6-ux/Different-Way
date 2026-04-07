extends Node2D

@onready var dialog_box = $UILayer/DialogBox
@onready var char_sheet = $UILayer/CharSheetUI
@onready var inventory_ui = $UILayer/InventoryUI
@onready var crafting_ui = $UILayer/CraftingUI
@onready var global_map = $UILayer/GlobalMap
@onready var console_ui = $UILayer/ConsoleUI
@onready var settings_ui = $UILayer/SettingsUI
@onready var combat_ui = $UILayer/CombatUI
@onready var merchant_ui = $UILayer/MerchantUI

@onready var active_quests_label = $UILayer/ActiveQuestsPanel/VBoxContainer/QuestsLabel
@onready var map_canvas = $UILayer/MapCanvas

const MAP_FILE_PATH = "user://local_map.json"

var locations_data = []
var roads_data = []

func _ready():
	_hide_all_windows()

	# Inject LLMClient into DialogBox
	var simulation = get_node_or_null("/root/Simulation")
	if simulation:
		var live_state = simulation.call("GetLiveState")
		if live_state and live_state.get("LlmClient") != null:
			dialog_box.llm_client_node = live_state.get("LlmClient")
			dialog_box.llm_client_node.AiResponseReceived.connect(_on_ai_response)

	$UILayer/HUD/CharButton.pressed.connect(func(): _toggle_window(char_sheet))
	$UILayer/HUD/InvButton.pressed.connect(func(): _toggle_window(inventory_ui))
	$UILayer/HUD/CraftButton.pressed.connect(func(): _toggle_window(crafting_ui))
	$UILayer/HUD/MapButton.pressed.connect(func(): _toggle_window(global_map))
	$UILayer/HUD/ConsoleButton.pressed.connect(func(): _toggle_window(console_ui))
	$UILayer/HUD/SettingsButton.pressed.connect(func(): _toggle_window(settings_ui))

	dialog_box.quest_accepted.connect(_on_quest_accepted)

	var event_bus = get_node_or_null("/root/EventBus")
	if event_bus:
		event_bus.MobKilled.connect(_on_mob_killed)
		event_bus.PlayerDied.connect(_on_player_died)
		event_bus.EncounterTriggered.connect(_on_encounter_triggered)

	_initialize_map()

func _initialize_map():
	if FileAccess.file_exists(MAP_FILE_PATH):
		_load_map()
	else:
		_generate_and_save_map()

	_render_map()

func _generate_and_save_map():
	# Procedurally generate coordinates for 4 key locations
	locations_data = [
		{"id": "plaza", "name": "Площадь (Староста)", "npc": "Староста", "x": 500, "y": 300},
		{"id": "tavern", "name": "Таверна (Алхимик)", "npc": "Алхимик", "x": 200, "y": 200},
		{"id": "smithy", "name": "Кузница (Кузнец)", "npc": "Кузнец", "x": 800, "y": 450},
		{"id": "forest", "name": "Лесная Опушка (Капитан)", "npc": "Капитан", "x": 850, "y": 150}
	]

	# Define roads connecting them
	roads_data = [
		{"from": "plaza", "to": "tavern"},
		{"from": "plaza", "to": "smithy"},
		{"from": "plaza", "to": "forest"},
		{"from": "smithy", "to": "forest"}
	]

	var data = {
		"locations": locations_data,
		"roads": roads_data
	}

	var file = FileAccess.open(MAP_FILE_PATH, FileAccess.WRITE)
	file.store_string(JSON.stringify(data, "\t"))
	file.close()

	var game_logger = get_node_or_null("/root/GameLogger")
	if game_logger: game_logger.call("WriteLog", "Generated and saved new map to " + MAP_FILE_PATH)
	else: print("Generated and saved new map to ", MAP_FILE_PATH)

func _load_map():
	var file = FileAccess.open(MAP_FILE_PATH, FileAccess.READ)
	var json_string = file.get_as_text()
	var data = JSON.parse_string(json_string)

	var game_logger = get_node_or_null("/root/GameLogger")

	if data and typeof(data) == TYPE_DICTIONARY:
		locations_data = data.get("locations", [])
		roads_data = data.get("roads", [])
		if game_logger: game_logger.call("WriteLog", "Loaded existing map from " + MAP_FILE_PATH)
		else: print("Loaded existing map from ", MAP_FILE_PATH)
	else:
		if game_logger: game_logger.call("WriteLogError", "Failed to parse map JSON.")
		else: printerr("Failed to parse map JSON.")
		_generate_and_save_map()

func _render_map():
	# Clear canvas just in case
	for child in map_canvas.get_children():
		child.queue_free()

	# Draw roads as lines
	for road in roads_data:
		var start_loc = _get_location_by_id(road["from"])
		var end_loc = _get_location_by_id(road["to"])

		if start_loc and end_loc:
			var line = Line2D.new()
			line.add_point(Vector2(start_loc["x"], start_loc["y"]))
			line.add_point(Vector2(end_loc["x"], end_loc["y"]))
			line.width = 6.0
			line.default_color = Color(0.4, 0.3, 0.2, 1) # Dirt road brown color
			map_canvas.add_child(line)

	# Draw locations as rectangle buttons (houses)
	for loc in locations_data:
		var btn = Button.new()
		btn.text = loc["name"]

		# Styling it like a rectangle building
		btn.custom_minimum_size = Vector2(160, 80)
		btn.position = Vector2(loc["x"] - 80, loc["y"] - 40) # Center on coordinate

		var npc_name = loc["npc"]
		btn.pressed.connect(func(): _interact_npc(npc_name))

		map_canvas.add_child(btn)

func _get_location_by_id(id: String):
	for loc in locations_data:
		if loc["id"] == id:
			return loc
	return null

func _hide_all_windows():
	dialog_box.hide()
	char_sheet.hide()
	inventory_ui.hide()
	crafting_ui.hide()
	global_map.hide()
	console_ui.hide()
	settings_ui.hide()
	combat_ui.hide()
	merchant_ui.hide()
	_update_time_pause()

func _toggle_window(window: Control):
	var is_visible = window.visible
	_hide_all_windows()
	window.visible = !is_visible
	_update_time_pause()

	if window == char_sheet and window.visible:
		var simulation = get_node_or_null("/root/Simulation")
		if simulation:
			var live_state = simulation.call("GetLiveState")
			if live_state:
				var player_stats = live_state.call("GetPlayerStats")
				if player_stats:
					char_sheet.update_character_sheet(player_stats)

	elif window == inventory_ui and window.visible:
		var simulation = get_node_or_null("/root/Simulation")
		if simulation:
			var live_state = simulation.call("GetLiveState")
			if live_state:
				var inv = live_state.call("GetPlayerInventory")
				if inv:
					inventory_ui.update_inventory(inv)

func _interact_npc(npc_name: String):
	# Directly interact with NPC inside the location
	_hide_all_windows()
	dialog_box.current_npc_name = npc_name
	dialog_box.show()
	_update_time_pause()

	var welcome_text = ""
	match npc_name:
		"Староста":
			welcome_text = "Староста: Добро пожаловать в Дубовую Гавань. Волки снова воют на опушке... (Квест: Убедить старосту)"
			_trigger_test_quest("Убедить старосту")
		"Кузнец":
			welcome_text = "Кузнец: Оружие затупилось? Неси материалы. Открываю лавку..."
			merchant_ui.open_shop("Кузнец")
		"Алхимик":
			welcome_text = "Алхимик: Мои зелья вернут тебя с того света... Открываю лавку..."
			merchant_ui.open_shop("Алхимик")
		"Капитан":
			welcome_text = "Капитан: Готов к бою? (Квест: Убить волков)"
			_start_actual_combat()

	dialog_box.get_node("ScrollContainer/ChatHistory").text = ""
	dialog_box.call("_append_chat", welcome_text)

func _trigger_test_quest(quest_title: String):
	var simulation = get_node_or_null("/root/Simulation")
	if simulation:
		var live_state = simulation.call("GetLiveState")
		if live_state:
			var quest_manager = live_state.call("GetQuestManager")
			if quest_manager:
				quest_manager.call("GenerateAndAcceptTestQuest", quest_title)
				_refresh_quests_ui(quest_manager)

func _on_player_died():
	_hide_all_windows()
	dialog_box.show()
	dialog_box.get_node("ScrollContainer/ChatHistory").text = ""

	var llm_msg = "Вас нашли без сознания на опушке леса и притащили в таверну. Вы потеряли часть золота, а ваше тело ломит от свежих травм. Вам нужно отдохнуть."
	dialog_box.call("_append_chat", "Алхимик: " + llm_msg)

func _on_mob_killed(mob_id: String):
	var simulation = get_node_or_null("/root/Simulation")
	if simulation:
		var live_state = simulation.call("GetLiveState")
		if live_state:
			var quest_manager = live_state.call("GetQuestManager")
			if quest_manager:
				quest_manager.call("UpdateQuestProgress", "dummy_target", 1) # Target dummy_target for the test quest
				_refresh_quests_ui(quest_manager)

func _refresh_quests_ui(quest_manager):
	var titles = quest_manager.call("GetActiveQuestTitles")
	var text = "Активные Квесты:\n"
	for t in titles:
		text += "- " + t + "\n"
	active_quests_label.text = text

func _on_encounter_triggered(enemy_id: String):
	_start_actual_combat(enemy_id)

func _start_actual_combat(custom_enemy_id: String = ""):
	var simulation = get_node_or_null("/root/Simulation")
	if not simulation: return

	# Instantiate a CombatManager
	var CSharpCombatManager = load("res://Systems/CombatManager.cs")
	var combat_manager = CSharpCombatManager.new()

	# Let Simulation build the combat entities and start combat
	if custom_enemy_id != "":
		simulation.call("StartEncounterCustom", combat_manager, custom_enemy_id)
	else:
		simulation.call("StartEncounter", combat_manager)

	_hide_all_windows()
	combat_ui.show()
	combat_ui.setup(combat_manager, simulation)
	_update_time_pause()

	var event_bus = get_node_or_null("/root/EventBus")
	if event_bus:
		event_bus.emit_signal("CombatStarted")

func _on_quest_accepted(quest_text: String):
	active_quests_label.text += "\n- " + quest_text

func _update_time_pause():
	var time_manager = get_node_or_null("/root/TimeManager")
	if time_manager:
		# Pause time if any blocking window is open (combat, dialogue, crafting, shop)
		var is_paused = dialog_box.visible or combat_ui.visible or merchant_ui.visible or crafting_ui.visible
		time_manager.set("IsPaused", is_paused)

func _on_ai_response(json_response: String):
	# Parse JSON response from LLM
	var json = JSON.new()
	var err = json.parse(json_response)
	if err == OK:
		var response_data = json.data
		var spoken_text = response_data.get("SpokenText", "...")
		var ai_thoughts = response_data.get("Thoughts", "")
		var actions = response_data.get("ActionTriggers", [])

		dialog_box.call("_append_chat", "NPC: " + spoken_text)

		# Log thoughts to console
		var logger = get_node_or_null("/root/GameLogger")
		if logger:
			logger.call("Log", "AI Thoughts: " + ai_thoughts)

		# Process Action Triggers from AI
		if actions != null and typeof(actions) == TYPE_ARRAY:
			var simulation = get_node_or_null("/root/Simulation")
			if simulation:
				var live_state = simulation.call("GetLiveState")
				var inv = live_state.call("GetPlayerInventory")
				var qm = live_state.call("GetQuestManager")

				for action in actions:
					if typeof(action) == TYPE_DICTIONARY:
						var type = action.get("Type", "")
						var amount = action.get("Amount", 0)
						var target_id = action.get("Id", "")

						if type == "give_gold":
							inv.call("AddGold", amount)
							if logger: logger.call("Log", "AI выдал золото: " + str(amount))
						elif type == "take_gold":
							inv.call("RemoveGold", amount)
							if logger: logger.call("Log", "AI забрал золото: " + str(amount))
						elif type == "give_item":
							inv.call("AddItem", target_id, amount)
							if logger: logger.call("Log", "AI выдал предмет: " + target_id + " x" + str(amount))
						elif type == "give_quest":
							var res = qm.call("AcceptQuestById", target_id)
							if res:
								_refresh_quests_ui(qm)
								if logger: logger.call("Log", "AI выдал квест: " + target_id)
						elif type == "complete_quest":
							var is_completed = qm.call("TryCompleteQuest", target_id, inv)
							if is_completed:
								_refresh_quests_ui(qm)
								if logger: logger.call("Log", "AI принял сдачу квеста: " + target_id)
