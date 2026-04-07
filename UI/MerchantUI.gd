extends Control

@onready var player_items_list = $MainLayout/PlayerPanel/PlayerItems
@onready var player_gold_label = $MainLayout/PlayerPanel/PlayerGold

@onready var merchant_items_list = $MainLayout/MerchantPanel/MerchantItems

@onready var buy_button = $MainLayout/CenterPanel/BuyButton
@onready var sell_button = $MainLayout/CenterPanel/SellButton

var current_merchant_stock = {}

func _ready():
	var close_btn = get_node_or_null("CloseButton")
	if close_btn: close_btn.pressed.connect(func(): hide())

	buy_button.pressed.connect(_on_buy_pressed)
	sell_button.pressed.connect(_on_sell_pressed)
	visibility_changed.connect(_on_visibility_changed)

func open_shop(merchant_type: String):
	# Generate a mock shop based on NPC
	if merchant_type == "Кузнец":
		current_merchant_stock = {"Железный меч": 1, "Боевой топор": 1, "Кожаная куртка": 2, "Ржавый меч": 3}
	elif merchant_type == "Алхимик":
		current_merchant_stock = {"Малое зелье здоровья": 5, "Малое зелье маны": 3, "Бинты": 10}
	else:
		current_merchant_stock = {"Хлеб": 10, "Деревянная дубина": 1}

	show()
	_refresh_ui()

func _on_visibility_changed():
	if visible:
		_refresh_ui()

func _refresh_ui():
	var simulation = get_node_or_null("/root/Simulation")
	if not simulation: return
	var live_state = simulation.call("GetLiveState")
	if not live_state: return

	# 1. Update Player Inventory side
	var inv = live_state.call("GetPlayerInventory")
	player_items_list.clear()
	if inv:
		player_gold_label.text = "Золото: " + str(inv.call("GetGold"))
		var items_data = inv.call("GetInventoryData")
		for item_id in items_data:
			var amount = items_data[item_id]
			var sell_price = live_state.call("GetItemSellPrice", item_id) # Needs wrapper
			var text = "%s (x%d) -> Продать за: %d" % [item_id, amount, sell_price]
			player_items_list.add_item(text)
			player_items_list.set_item_metadata(player_items_list.get_item_count() - 1, {"id": item_id, "price": sell_price})

	# 2. Update Merchant side
	merchant_items_list.clear()
	for item_id in current_merchant_stock:
		var amount = current_merchant_stock[item_id]
		if amount > 0:
			var buy_price = live_state.call("GetItemBuyPrice", item_id) # Needs wrapper
			var text = "%s (В наличии: %d) -> Купить за: %d" % [item_id, amount, buy_price]
			merchant_items_list.add_item(text)
			merchant_items_list.set_item_metadata(merchant_items_list.get_item_count() - 1, {"id": item_id, "price": buy_price})

func _on_buy_pressed():
	var selected = merchant_items_list.get_selected_items()
	if selected.size() > 0:
		var meta = merchant_items_list.get_item_metadata(selected[0])
		var simulation = get_node_or_null("/root/Simulation")
		if simulation:
			var live_state = simulation.call("GetLiveState")
			if live_state:
				var success = live_state.call("AttemptBuyFromUI", meta.id, meta.price)
				if success:
					current_merchant_stock[meta.id] -= 1
					_refresh_ui()

func _on_sell_pressed():
	var selected = player_items_list.get_selected_items()
	if selected.size() > 0:
		var meta = player_items_list.get_item_metadata(selected[0])
		var simulation = get_node_or_null("/root/Simulation")
		if simulation:
			var live_state = simulation.call("GetLiveState")
			if live_state:
				var success = live_state.call("AttemptSellFromUI", meta.id, meta.price)
				if success:
					# Add back to merchant stock if sold
					if current_merchant_stock.has(meta.id): current_merchant_stock[meta.id] += 1
					else: current_merchant_stock[meta.id] = 1
					_refresh_ui()
