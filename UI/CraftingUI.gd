extends Control

# 7.6. Меню Крафта
# Дерево рецептов слева, требуемые материалы по центру (красный текст при нехватке),
# и прогноз шанса успеха справа.

@onready var recipe_tree = $HBoxContainer/RecipeTree
@onready var materials_list = $HBoxContainer/MaterialsCenter/VBoxContainer
@onready var success_forecast = $HBoxContainer/ForecastRight/VBoxContainer/PercentLabel
@onready var craft_btn = $HBoxContainer/ForecastRight/VBoxContainer/CraftButton

var current_recipe = null
var csharp_crafting_engine
var recipes_dict = {}

func _ready():
	var close_btn = get_node_or_null("CloseButton")
	if close_btn: close_btn.pressed.connect(func(): hide())

	recipe_tree.item_selected.connect(_on_tree_item_selected)
	if craft_btn: craft_btn.pressed.connect(_on_craft_button_pressed)

	visibility_changed.connect(_on_visibility_changed)

func _on_visibility_changed():
	if visible:
		_populate_tree()

func _populate_tree():
	recipe_tree.clear()
	var root = recipe_tree.create_item()
	recipe_tree.hide_root = true

	var simulation = get_node_or_null("/root/Simulation")
	if not simulation: return

	var live_state = simulation.call("GetLiveState")
	if not live_state: return

	csharp_crafting_engine = live_state.get("CraftingEngine")
	recipes_dict = live_state.call("GetRecipes")

	for recipe_id in recipes_dict:
		var item = recipe_tree.create_item(root)
		var recipe = recipes_dict[recipe_id]
		item.set_text(0, recipe.name)
		item.set_metadata(0, recipe_id)

func _on_tree_item_selected():
	var selected = recipe_tree.get_selected()
	if selected:
		var r_id = selected.get_metadata(0)
		var recipe_data = recipes_dict[r_id]

		var simulation = get_node_or_null("/root/Simulation")
		if simulation:
			var live_state = simulation.call("GetLiveState")
			if live_state:
				var inv = live_state.call("GetPlayerInventory").call("GetInventoryData")
				var p_int = live_state.call("GetPlayerStats").call("GetINT")
				select_recipe(recipe_data, inv, p_int)

func select_recipe(recipe_data, player_inventory, player_int):
	current_recipe = recipe_data
	_render_materials(recipe_data.requirements, player_inventory)
	_render_forecast(recipe_data, player_int)

func _render_materials(requirements, inventory):
	for child in materials_list.get_children(): child.queue_free()

	for req in requirements:
		var has_amount = inventory.get(req.id, 0)
		var label = Label.new()

		if has_amount < req.amount:
			label.add_theme_color_override("font_color", Color(1, 0, 0)) # Red
		else:
			label.add_theme_color_override("font_color", Color(1, 1, 1)) # White

		label.text = req.name + ": " + str(has_amount) + "/" + str(req.amount)
		materials_list.add_child(label)

func _render_forecast(recipe, player_int):
	if csharp_crafting_engine:
		var chance = csharp_crafting_engine.call("CalculateSuccessChance", recipe.difficulty, player_int)
		success_forecast.text = "Шанс успеха: " + str(chance) + "%"

func _on_craft_button_pressed():
	if not current_recipe: return

	var simulation = get_node_or_null("/root/Simulation")
	if simulation:
		var live_state = simulation.call("GetLiveState")
		if live_state:
			live_state.call("AttemptCraftingFromUI", current_recipe.id)
			_on_tree_item_selected() # Refresh materials
