extends Control

# 7.4. Меню Персонажа (CharSheetUI)
# Детально отражает состояние "Скелета". Блоки: Аватарка/Уровень, Основные статы (STR/DEX),
# Вторички (Уворот/Мент.Сопр), Активные травмы и баффы с таймерами.

@onready var avatar_rect = $AvatarRect
@onready var level_label = $LevelLabel
@onready var str_label = $StatsContainer/STRLabel
@onready var dex_label = $StatsContainer/DEXLabel
@onready var evasion_label = $SecondaryContainer/EvasionLabel
@onready var mental_res_label = $SecondaryContainer/MentalResLabel
@onready var buffs_container = $BuffsScroll/VBoxContainer

# Assume reference to C# StatManager node
var stat_manager

func _ready():
	# Memory: Ensure container labels can autowrap to avoid layout breaking
	str_label.autowrap_mode = TextServer.AUTOWRAP_WORD
	dex_label.autowrap_mode = TextServer.AUTOWRAP_WORD

func update_character_sheet(stats):
	if stats == null: return

	str_label.text = "STR: " + str(stats.Call("get_STR"))
	dex_label.text = "DEX: " + str(stats.Call("get_DEX"))

	evasion_label.text = "Уворот: " + str(stats.Call("get_Evasion"))
	mental_res_label.text = "Мент. Защ: " + str(stats.Call("get_MentalResistance"))

	_update_buffs(stats)

func _update_buffs(stats):
	# Clear children
	for child in buffs_container.get_children():
		child.queue_free()

	# Populate active trauma and buffs
	# This would call into StatusEffectManager
