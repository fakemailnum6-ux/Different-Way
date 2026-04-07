extends Control

signal action_selected(action_type, npc_name)

@onready var title_label = $Panel/VBoxContainer/TitleLabel
@onready var talk_btn = $Panel/VBoxContainer/TalkButton
@onready var trade_btn = $Panel/VBoxContainer/TradeButton
@onready var quest_btn = $Panel/VBoxContainer/QuestButton
@onready var attack_btn = $Panel/VBoxContainer/AttackButton
@onready var close_btn = $Panel/VBoxContainer/CloseButton

var current_npc_name = ""

func _ready():
	talk_btn.pressed.connect(func(): _on_action("talk"))
	trade_btn.pressed.connect(func(): _on_action("trade"))
	quest_btn.pressed.connect(func(): _on_action("quest"))
	attack_btn.pressed.connect(func(): _on_action("attack"))
	close_btn.pressed.connect(func(): hide())

func open_menu(npc_name: String):
	current_npc_name = npc_name
	title_label.text = "Взаимодействие: " + npc_name

	# Show/Hide specific buttons depending on NPC
	trade_btn.visible = (npc_name in ["Кузнец", "Алхимик", "Торговец"])
	quest_btn.visible = (npc_name in ["Староста", "Капитан"])

	# Only bandits/monsters have attack by default, but let's allow attacking anyone for sandbox
	attack_btn.visible = true

	show()

func _on_action(action: String):
	action_selected.emit(action, current_npc_name)
	hide()
