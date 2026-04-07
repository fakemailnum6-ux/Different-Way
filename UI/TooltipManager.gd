extends Control

# 7.5 Hover Compare Logic

@onready var equipped_tooltip = $EquippedTooltip
@onready var hovered_tooltip = $HoveredTooltip

func _ready():
	hide()

func show_comparison(equipped_item_stats, hovered_item_stats):
	show()
	_render_tooltip(equipped_tooltip, equipped_item_stats, null)
	_render_tooltip(hovered_tooltip, hovered_item_stats, equipped_item_stats)

	# Positioning near mouse
	global_position = get_global_mouse_position() + Vector2(15, 15)

func _render_tooltip(tooltip_node, stats, comparison_base):
	if stats == null or stats.size() == 0:
		tooltip_node.hide()
		return

	tooltip_node.show()

	# Formatting for green/red coloring using BBCode
	var text = "[b]" + str(stats.get("name", "Unknown")) + "[/b]\n\n"

	if stats.get("type") == "weapon" and stats.has("damage"):
		var dmg = stats.damage
		var color = "white"

		# Ensure comparison_base is a valid dictionary and has damage
		if typeof(comparison_base) == TYPE_DICTIONARY and comparison_base.has("damage"):
			if dmg > comparison_base.damage: color = "green"
			elif dmg < comparison_base.damage: color = "red"

		text += "Урон: [color=" + color + "]" + str(dmg) + "[/color]\n"

	if stats.get("type") == "armor" and stats.has("armor"):
		var arm = stats.armor
		var color = "white"

		if typeof(comparison_base) == TYPE_DICTIONARY and comparison_base.has("armor"):
			if arm > comparison_base.armor: color = "green"
			elif arm < comparison_base.armor: color = "red"

		text += "Броня: [color=" + color + "]" + str(arm) + "[/color]\n"

	if stats.has("desc") and stats.desc != "":
		text += "\n[i]" + str(stats.desc) + "[/i]\n"

	if stats.has("price"):
		text += "\nЦена: " + str(stats.price) + "g"

	tooltip_node.get_node("RichTextLabel").text = text

func hide_tooltips():
	hide()
