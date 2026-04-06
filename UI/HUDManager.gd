extends Control

@onready var time_label = $Panel/HBoxContainer/TimeLabel
@onready var gold_label = $Panel/HBoxContainer/GoldLabel
@onready var hp_label = $Panel/HBoxContainer/HPLabel
@onready var stamina_label = $Panel/HBoxContainer/StaminaLabel
@onready var mana_label = $Panel/HBoxContainer/ManaLabel

@onready var refresh_timer = $RefreshTimer

func _ready():
	refresh_timer.timeout.connect(_refresh_stats)
	_refresh_stats()

func _refresh_stats():
	var simulation = get_node_or_null("/root/Simulation")
	var time_manager = get_node_or_null("/root/TimeManager")

	if time_manager:
		time_label.text = time_manager.call("GetFormattedTime")

	if simulation:
		var live_state = simulation.call("GetLiveState")
		if live_state:
			var stats = live_state.call("GetPlayerStats")
			if stats:
				hp_label.text = "HP: %d/%d" % [stats.call("GetCurrentHP"), stats.call("GetMaxHP")]
				stamina_label.text = "SP: %d/%d" % [stats.call("GetCurrentStamina"), stats.call("GetMaxStamina")]
				mana_label.text = "MP: %d/%d" % [stats.call("GetCurrentMana"), stats.call("GetMaxMana")]

			var inv = live_state.call("GetPlayerInventory")
			if inv:
				gold_label.text = "Золото: " + str(inv.call("GetGold"))
