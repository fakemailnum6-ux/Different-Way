extends Control

@onready var player_hp_label = $MainLayout/LeftPanel/PlayerHP
@onready var timer_label = $MainLayout/LeftPanel/ActionPanel/VBoxContainer/TimerLabel
@onready var attack_btn = $MainLayout/LeftPanel/ActionPanel/VBoxContainer/AttackButton
@onready var defend_btn = $MainLayout/LeftPanel/ActionPanel/VBoxContainer/DefendButton
@onready var flee_btn = $MainLayout/LeftPanel/ActionPanel/VBoxContainer/FleeButton

@onready var combat_log = $MainLayout/CenterPanel/CombatLog
@onready var enemies_list = $MainLayout/RightPanel/EnemiesList
@onready var turn_timer = $TurnTimer

var combat_manager = null
var simulation = null
var selected_enemy_id = ""

var is_player_turn = false
var time_left = 10.0

func _ready():
	attack_btn.pressed.connect(_on_attack_pressed)
	defend_btn.pressed.connect(_on_defend_pressed)
	flee_btn.pressed.connect(_on_flee_pressed)
	turn_timer.timeout.connect(_on_timer_tick)

func setup(cm, sim):
	combat_manager = cm
	simulation = sim

	if not combat_manager.is_connected("CombatLogUpdated", _on_log_updated):
		combat_manager.CombatLogUpdated.connect(_on_log_updated)
		combat_manager.TurnChanged.connect(_on_turn_changed)
		combat_manager.CombatEnded.connect(_on_combat_ended)
		combat_manager.EntityStatusChanged.connect(refresh_ui)

	combat_log.text = ""
	refresh_ui()

func refresh_ui():
	if not simulation: return
	var state = simulation.call("GetLiveState")
	var stats = state.call("GetPlayerStats")
	if stats:
		player_hp_label.text = "HP: %d/%d" % [stats.call("GetCurrentHP"), stats.call("GetMaxHP")]

	_update_enemies_list()

func _update_enemies_list():
	for child in enemies_list.get_children():
		child.queue_free()

	if not combat_manager: return

	var enemies = combat_manager.call("GetEnemyList")
	for e in enemies:
		var btn = Button.new()
		btn.text = "%s (%s) HP: %d/%d" % [e["name"], e["zone"], e["hp"], e["max_hp"]]
		btn.add_theme_font_size_override("font_size", 16)
		if e["hp"] <= 0:
			btn.disabled = true
			btn.text += " [МЕРТВ]"

		btn.pressed.connect(func(): selected_enemy_id = e["id"])
		enemies_list.add_child(btn)

func _on_turn_changed(is_player: bool, entity_name: String):
	is_player_turn = is_player
	attack_btn.disabled = !is_player
	defend_btn.disabled = !is_player
	flee_btn.disabled = !is_player

	if is_player:
		time_left = 10.0
		timer_label.text = "10.0"
		timer_label.add_theme_color_override("font_color", Color(1, 1, 0))
		turn_timer.start()
	else:
		turn_timer.stop()
		timer_label.text = "Ждите..."
		timer_label.add_theme_color_override("font_color", Color(0.5, 0.5, 0.5))

func _on_timer_tick():
	if is_player_turn:
		time_left -= 0.1
		if time_left <= 0:
			time_left = 0
			turn_timer.stop()
			_on_defend_pressed() # Auto-defend if out of time

		timer_label.text = "%.1f" % time_left
		if time_left <= 3.0:
			timer_label.add_theme_color_override("font_color", Color(1, 0, 0))

func _on_attack_pressed():
	if selected_enemy_id == "":
		_on_log_updated("Выберите цель справа!")
		return
	turn_timer.stop()
	combat_manager.call("PlayerAttackTarget", selected_enemy_id)

func _on_defend_pressed():
	turn_timer.stop()
	combat_manager.call("PlayerDefend")

func _on_flee_pressed():
	turn_timer.stop()
	combat_manager.call("AttemptFlee")

func _on_log_updated(msg: String):
	combat_log.append_text(msg + "\n")

func _on_combat_ended(player_won: bool):
	turn_timer.stop()
	is_player_turn = false
	attack_btn.disabled = true
	defend_btn.disabled = true
	flee_btn.disabled = true

	var event_bus = get_node_or_null("/root/EventBus")
	if event_bus:
		event_bus.emit_signal("CombatEnded", player_won)

	# Close UI after delay (handled by map usually, but can be done here)
	await get_tree().create_timer(3.0).timeout
	hide()
