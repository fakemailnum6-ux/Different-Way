extends Control

signal closed

@onready var player_hp_label = $Panel/VBoxContainer/HBoxContainer/PlayerHP
@onready var enemy_hp_label = $Panel/VBoxContainer/HBoxContainer/EnemyHP
@onready var combat_log = $Panel/VBoxContainer/CombatLog
@onready var attack_button = $Panel/VBoxContainer/HBoxContainer2/AttackButton

var max_player_hp = 20
var player_hp = 20
var max_enemy_hp = 10
var enemy_hp = 10

func _ready():
	_update_ui()

func start_combat():
	player_hp = max_player_hp
	enemy_hp = max_enemy_hp
	combat_log.text = "You entered the cellar. Giant rats approach!\n"
	attack_button.disabled = false
	_update_ui()

func _update_ui():
	player_hp_label.text = "Player HP: " + str(player_hp) + "/" + str(max_player_hp)
	enemy_hp_label.text = "Rats HP: " + str(enemy_hp) + "/" + str(max_enemy_hp)

func _log(msg: String):
	combat_log.text += msg + "\n"

func _on_attack_button_pressed():
	# Player attacks
	var player_roll = randi() % 20 + 1
	var player_damage = 0

	if player_roll >= 10:
		player_damage = randi() % 4 + 1 # 1d4 damage
		enemy_hp -= player_damage
		_log("You rolled " + str(player_roll) + " and hit for " + str(player_damage) + " damage.")
	else:
		_log("You rolled " + str(player_roll) + " and missed.")

	_update_ui()

	if enemy_hp <= 0:
		_log("You defeated the rats!")
		attack_button.disabled = true
		return

	# Rats attack back
	var enemy_roll = randi() % 20 + 1
	var enemy_damage = 0

	if enemy_roll >= 12:
		enemy_damage = randi() % 3 + 1 # 1d3 damage
		player_hp -= enemy_damage
		_log("Rats rolled " + str(enemy_roll) + " and hit you for " + str(enemy_damage) + " damage.")
	else:
		_log("Rats rolled " + str(enemy_roll) + " and missed.")

	_update_ui()

	if player_hp <= 0:
		_log("You were defeated by the rats...")
		attack_button.disabled = true

func _on_flee_button_pressed():
	closed.emit()
