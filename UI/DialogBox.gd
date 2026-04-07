extends Control

signal prompt_sent(prompt_text)
signal dialog_closed()
signal quest_accepted(quest_text)

@onready var scroll_container = $ScrollContainer
@onready var chat_history_label = $ScrollContainer/ChatHistory
@onready var input_field = $InputPanel/LineEdit
@onready var send_button = $InputPanel/SendButton
@onready var accept_quest_button = $AcceptQuestButton

var llm_client_node = null
var current_npc_name = ""

func _ready():
	chat_history_label.autowrap_mode = TextServer.AUTOWRAP_WORD
	send_button.pressed.connect(_on_send_pressed)

	var close_button = $CloseButton
	if close_button:
		close_button.pressed.connect(close_dialog)

	if accept_quest_button:
		accept_quest_button.pressed.connect(_on_accept_quest)
		accept_quest_button.hide()

func _on_send_pressed():
	var prompt = input_field.text
	if prompt.strip_edges() == "": return

	input_field.text = ""
	_append_chat("Вы: " + prompt)

	if llm_client_node:
		llm_client_node.call("RequestPromptAsync", prompt, current_npc_name)

func _append_chat(text: String):
	chat_history_label.text += text + "\n"
	await get_tree().process_frame
	scroll_container.scroll_vertical = scroll_container.get_v_scroll_bar().max_value

	# Show "Accept Quest" button if dialogue implies a quest
	if "(Квест:" in text:
		accept_quest_button.show()

func _on_accept_quest():
	quest_accepted.emit("Новый Квест Принят!")
	accept_quest_button.hide()

func close_dialog():
	if llm_client_node:
		llm_client_node.call("CancelPendingRequests")
	dialog_closed.emit()
	hide()
