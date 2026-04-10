extends Node2D

@onready var map_layer = $UILayer/MapLayer
@onready var menu_layer = $UILayer/MenuLayer
@onready var action_layer = $UILayer/ActionLayer
@onready var hud_layer = $UILayer/HUDLayer
@onready var console_layer = $UILayer/ConsoleLayer

var event_bus: Node
var game_state_machine: Node

enum GameState {
    Loading,
    Exploration,
    Dialogue,
    Combat,
    Menu
}

func _ready() -> void:
    event_bus = get_node("/root/EventBus")
    game_state_machine = get_node("/root/GameStateMachine")

    if event_bus:
        event_bus.connect("state_transitioned", Callable(self, "_on_state_transitioned"))

    _update_ui_for_state(GameState.Loading)

func _on_state_transitioned(new_state_enum: int) -> void:
    _update_ui_for_state(new_state_enum)

func _update_ui_for_state(state: int) -> void:
    # Hide all context layers by default
    map_layer.hide()
    menu_layer.hide()
    action_layer.hide()

    # HUD and Console are generally always visible depending on toggle,
    # but we enforce visibility here for safety.
    hud_layer.show()
    console_layer.show()

    if state == GameState.Loading:
        hud_layer.hide() # Maybe show a loading spinner in action layer later
        action_layer.show()
    elif state == GameState.Exploration:
        map_layer.show()
    elif state == GameState.Dialogue:
        map_layer.show() # Keep map visible in background
        action_layer.show() # Dialog box goes here
    elif state == GameState.Combat:
        action_layer.show() # Combat UI goes here
    elif state == GameState.Menu:
        map_layer.show()
        menu_layer.show() # Inventory / CharSheet

func _unhandled_input(event: InputEvent) -> void:
    if event.is_action_pressed("ui_cancel"):
        _close_all_non_blocking_windows()

func _close_all_non_blocking_windows() -> void:
    var current_state = game_state_machine.call("GetCurrentState") if game_state_machine else GameState.Exploration

    if current_state == GameState.Menu:
        event_bus.call("Emit", "request_state_change", GameState.Exploration)

    # Per Arc.md, do not close dialogue or combat windows with Escape
