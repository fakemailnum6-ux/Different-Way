extends Control

func _ready():
    # Make sure text wraps to prevent overlapping
    if has_node("Label"):
        $Label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
