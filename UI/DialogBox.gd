extends Control

# 7.2. Асинхронные конфликты (Task Cancellation)
# При отправке запроса в ИИ передается CancellationToken. Если игрок закроет окно DialogBox
# не дождавшись ответа, токен отменит запрос, предотвратив попытку ИИ записать данные в выгруженный UI.

signal prompt_sent(prompt_text)
signal dialog_closed()

@onready var scroll_container = $ScrollContainer
@onready var chat_history_label = $ScrollContainer/ChatHistory
@onready var input_field = $InputPanel/LineEdit
@onready var send_button = $InputPanel/SendButton

# Reference to the C# LLMClient node/manager
var llm_client_node = null

func _ready():
	# 7.1. Anti-Overlap: Весь ИИ-текст обернут в ScrollContainer с autowrap_mode.
	chat_history_label.autowrap_mode = TextServer.AUTOWRAP_WORD

	send_button.pressed.connect(_on_send_pressed)

	# Close button
	var close_button = $CloseButton
	if close_button:
		close_button.pressed.connect(close_dialog)

func _on_send_pressed():
	var prompt = input_field.text
	if prompt.strip_edges() == "": return

	input_field.text = ""
	_append_chat("Вы: " + prompt)

	if llm_client_node:
		# Calling C# async method, generating a cancellation token locally or relying on C# to return an ID
		llm_client_node.Call("RequestPromptAsync", prompt)

func _append_chat(text: String):
	chat_history_label.text += text + "\n"
	# Auto-scroll to bottom
	await get_tree().process_frame
	scroll_container.scroll_vertical = scroll_container.get_v_scroll_bar().max_value

func close_dialog():
	# Cancel pending AI requests to prevent crashes
	if llm_client_node:
		llm_client_node.Call("CancelPendingRequests")

	dialog_closed.emit()
	hide()
