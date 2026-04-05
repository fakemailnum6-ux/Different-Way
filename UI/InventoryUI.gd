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

func populate_inventory(items_data):
	# Render inventory icons (ASCII/UTF-8 symbols as per Code-Only rendering restriction)
	# using a ColorRect and RichTextLabel
	pass

func _on_item_mouse_entered(item_data):
	# 7.5 Hover Compare Trigger
	item_hovered.emit(item_data.id, item_data.type)

func _on_item_mouse_exited():
	item_unhovered.emit()
