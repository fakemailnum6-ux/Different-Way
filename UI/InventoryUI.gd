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

			var dummy_item = {"id": item_id, "type": "weapon"}
			btn.mouse_entered.connect(func(): _on_item_mouse_entered(dummy_item))
			btn.mouse_exited.connect(func(): _on_item_mouse_exited())

			weapons_tab.add_child(btn)

func _on_item_mouse_entered(item_data):
	# 7.5 Hover Compare Trigger
	item_hovered.emit(item_data.id, item_data.type)

func _on_item_mouse_exited():
	item_unhovered.emit()
