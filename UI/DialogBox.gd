extends PanelContainer

@onready var text_label = $MarginContainer/VBoxContainer/TextLabel
@onready var close_button = $MarginContainer/VBoxContainer/CloseButton

func _ready():
	hide()
	close_button.pressed.connect(_on_close_pressed)

func show_dialog(text: String):
	text_label.text = text
	show()

func _on_close_pressed():
	hide()
