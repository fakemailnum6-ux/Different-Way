extends Control

# 7.4. Меню Персонажа (CharSheetUI)
# Детально отражает состояние "Скелета". Блоки: Аватарка/Уровень, Основные статы (STR/DEX),
# Вторички (Уворот/Мент.Сопр), Активные травмы и баффы с таймерами.

@onready var avatar_rect = $AvatarRect
@onready var level_label = $LevelLabel

@onready var str_label = $StatsContainer/STRLabel
@onready var dex_label = $StatsContainer/DEXLabel
@onready var end_label = $StatsContainer/ENDLabel
@onready var int_label = $StatsContainer/INTLabel
@onready var luck_label = $StatsContainer/LuckLabel
@onready var cha_label = $StatsContainer/CHALabel

@onready var hp_label = $SecondaryContainer/HPLabel
@onready var stamina_label = $SecondaryContainer/StaminaLabel
@onready var mana_label = $SecondaryContainer/ManaLabel
@onready var evasion_label = $SecondaryContainer/EvasionLabel
@onready var mental_res_label = $SecondaryContainer/MentalResLabel
@onready var cardio_label = $SecondaryContainer/CardioLabel

@onready var buffs_container = $BuffsScroll/VBoxContainer

# Assume reference to C# StatManager node
var stat_manager

func _ready():
	# Memory: Ensure container labels can autowrap to avoid layout breaking
	str_label.autowrap_mode = TextServer.AUTOWRAP_WORD
	dex_label.autowrap_mode = TextServer.AUTOWRAP_WORD

	# Connect Close Button
	var close_button = $CloseButton
	if close_button:
		close_button.pressed.connect(func(): hide())

func update_character_sheet(stats):
	if stats == null: return

	str_label.text = "STR: " + str(stats.call("GetSTR"))
	dex_label.text = "DEX: " + str(stats.call("GetDEX"))
	end_label.text = "END: " + str(stats.call("GetEND"))
	int_label.text = "INT: " + str(stats.call("GetINT"))
	luck_label.text = "Удача: " + str(stats.call("GetLuck"))
	cha_label.text = "Харизма: " + str(stats.call("GetCharisma"))

	hp_label.text = "HP: " + str(stats.call("GetCurrentHP")) + "/" + str(stats.call("GetMaxHP"))
	stamina_label.text = "Усталость: " + str(stats.call("GetCurrentStamina")) + "/" + str(stats.call("GetMaxStamina"))
	mana_label.text = "Мана: " + str(stats.call("GetCurrentMana")) + "/" + str(stats.call("GetMaxMana"))

	evasion_label.text = "Уворот: " + str(stats.call("GetEvasion"))
	mental_res_label.text = "Мент. Защ: " + str(stats.call("GetMentalResistance"))
	cardio_label.text = "Кардио: " + str(stats.call("GetCardio"))

	_update_buffs(stats)

func _update_buffs(stats):
	# Clear children
	for child in buffs_container.get_children():
		child.queue_free()
