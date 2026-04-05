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
	# Example formatting for green/red coloring using BBCode
	var text = "[b]" + stats.name + "[/b]\n"

	if stats.has("damage"):
		var dmg = stats.damage
		var color = "white"

		if comparison_base != null and comparison_base.has("damage"):
			if dmg > comparison_base.damage: color = "green"
			elif dmg < comparison_base.damage: color = "red"

		text += "[color=" + color + "]Урон: " + str(dmg) + "[/color]\n"

	tooltip_node.get_node("RichTextLabel").text = text

func hide_tooltips():
	hide()
