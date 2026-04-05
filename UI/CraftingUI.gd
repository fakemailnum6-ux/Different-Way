extends Control

# 7.6. Меню Крафта
# Дерево рецептов слева, требуемые материалы по центру (красный текст при нехватке),
# и прогноз шанса успеха справа.

@onready var recipe_tree = $HBoxContainer/RecipeTree
@onready var materials_list = $HBoxContainer/MaterialsCenter/VBoxContainer
@onready var success_forecast = $HBoxContainer/ForecastRight/PercentLabel

var current_recipe = null
var csharp_crafting_engine

func _ready():
	var close_btn = get_node_or_null("CloseButton")
	if close_btn: close_btn.pressed.connect(func(): hide())

	pass

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
	# Math from C# CraftingEngine
	var chance = csharp_crafting_engine.call("CalculateSuccessChance", recipe.difficulty, player_int)
	success_forecast.text = "Шанс успеха: " + str(chance) + "%"
