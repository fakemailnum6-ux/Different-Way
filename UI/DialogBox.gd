extends Control

func _ready():
    # Make sure text wraps to prevent overlapping
    if has_node("Panel/Label"):
        $Panel/Label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
