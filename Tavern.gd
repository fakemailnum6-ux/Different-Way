extends Control

signal return_to_map

var quest_status = "none" # none, active, completed

@onready var dialog_box = $DialogBox
@onready var quest_log = $QuestLog
@onready var talk_button = $TalkButton
@onready var complete_button = $CompleteQuestButton
@onready var return_button = $ReturnButton

func _ready():
	talk_button.pressed.connect(_on_talk_pressed)
	complete_button.pressed.connect(_on_complete_pressed)
	return_button.pressed.connect(_on_return_pressed)

func _on_talk_pressed():
	if quest_status == "none":
		dialog_box.show_dialog("Bob: 'Hey traveler! We got a rat problem in the cellar. Bring me 5 rat tails and I'll give you a free ale.'")
		quest_status = "active"
		quest_log.text = "--- Quest Log ---\n[Active] Rat Problem: Collect 5 rat tails for Bob."
		complete_button.disabled = false
	elif quest_status == "active":
		dialog_box.show_dialog("Bob: 'Did you get those rat tails yet? The cellar is still squeaking!'")
	elif quest_status == "completed":
		dialog_box.show_dialog("Bob: 'Thanks for the help, friend. Enjoy your ale!'")

func _on_complete_pressed():
	if quest_status == "active":
		dialog_box.show_dialog("Bob: 'Ah, perfect! Here is your ale as promised.'")
		quest_status = "completed"
		quest_log.text = "--- Quest Log ---\n[Completed] Rat Problem"
		complete_button.disabled = true

func _on_return_pressed():
	return_to_map.emit()
