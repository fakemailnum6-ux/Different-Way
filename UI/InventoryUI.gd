extends Control

# 7.5. Инвентарь и Tooltip Сравнение
# Вкладки: Оружие, Броня, Материалы, Еда/Зелья, Квестовые предметы.
# Hover Compare: Разница подсвечивается зеленым/красным цветом.

signal item_hovered(item_id, slot_type)
signal item_unhovered()

@onready var weapons_tab = $TabContainer/Weapons
@onready var armor_tab = $TabContainer/Armor
@onready var materials_tab = $TabContainer/Materials
@onready var consumables_tab = $TabContainer/Consumables
@onready var tooltip_manager = $"../TooltipManager"

func _ready():
	var close_btn = get_node_or_null("CloseButton")
	if close_btn: close_btn.pressed.connect(func(): hide())

	# Setup filter and sorting buttons here
	pass

func update_inventory(inventory_manager):
	for tab in [weapons_tab, armor_tab, materials_tab, consumables_tab]:
		if tab:
			for child in tab.get_children():
				child.queue_free()

	if not inventory_manager: return

	var items_data = inventory_manager.call("GetInventoryData")

	if items_data.size() == 0 and weapons_tab:
		var empty_lbl = Label.new()
		empty_lbl.text = "Инвентарь пуст."
		weapons_tab.add_child(empty_lbl)
		return

	# Simple list rendering on the first tab for now
	if weapons_tab:
		for item_id in items_data:
			var amount = items_data[item_id]
			var btn = Button.new()
			btn.text = item_id + " (x" + str(amount) + ")"

			btn.mouse_entered.connect(func(): _on_item_mouse_entered(item_id))
			btn.mouse_exited.connect(func(): _on_item_mouse_exited())
			btn.pressed.connect(func(): _on_item_pressed(item_id))

			weapons_tab.add_child(btn)

func _on_item_pressed(item_id: String):
	var simulation = get_node_or_null("/root/Simulation")
	if simulation:
		var live_state = simulation.call("GetLiveState")
		if live_state:
			live_state.call("AttemptEquipFromUI", item_id)
			# Refresh the inventory or character sheet logic
			var player_inv = live_state.call("GetPlayerInventory")
			if player_inv:
				update_inventory(player_inv)

func _on_item_mouse_entered(item_id: String):
	var simulation = get_node_or_null("/root/Simulation")
	if simulation:
		var live_state = simulation.call("GetLiveState")
		if live_state:
			var item_stats = live_state.call("GetItemStats", item_id)
			var equipped_item_name = ""
			var equipped_stats = {}

			var equipment = live_state.call("GetPlayerEquipment")
			if item_stats.get("type") == "weapon":
				equipped_item_name = equipment.call("GetEquippedWeaponName")
			elif item_stats.get("type") == "armor":
				equipped_item_name = equipment.call("GetEquippedArmorName")

			if equipped_item_name != "" and equipped_item_name != "Безоружный" and equipped_item_name != "Лохмотья":
				equipped_stats = live_state.call("GetItemStats", equipped_item_name)

			if tooltip_manager:
				tooltip_manager.show_comparison(equipped_stats, item_stats)

func _on_item_mouse_exited():
	if tooltip_manager:
		tooltip_manager.hide_tooltips()
	item_unhovered.emit()
