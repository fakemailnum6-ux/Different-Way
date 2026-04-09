using Godot;
using System;
using System.Collections.Generic;
using DifferentWay.Core;

namespace DifferentWay.Systems
{
    public partial class MerchantLogic : Node
    {
        private EventBus _eventBus;

        public override void _Ready()
        {
            _eventBus = GetNodeOrNull<EventBus>("/root/EventBus");
        }

        // Returns dynamic buy price based on Charisma discount
        public int GetBuyPrice(int basePrice, CharacterStats buyerStats)
        {
            float discount = buyerStats.Charisma * 0.02f; // 2% discount per CHR
            if (discount > 0.5f) discount = 0.5f; // Max 50% discount

            int price = Mathf.CeilToInt(basePrice * (1.0f - discount));
            return Mathf.Max(1, price); // Min 1 gold
        }

        // Returns sell price (markup for player selling to merchant)
        public int GetSellPrice(int basePrice, CharacterStats sellerStats)
        {
            float bonus = sellerStats.Charisma * 0.01f; // 1% bonus per CHR
            if (bonus > 0.3f) bonus = 0.3f; // Max 30% extra value

            // Base sell value is usually 50% of base price
            int price = Mathf.CeilToInt(basePrice * 0.5f * (1.0f + bonus));
            return Mathf.Max(0, price); // Can be 0 for trash
        }

        public bool PerformTransaction(CharacterStats buyer, Dictionary<string, int> buyerInventory, ref int buyerGold, string itemId, int basePrice)
        {
            int cost = GetBuyPrice(basePrice, buyer);

            if (buyerGold >= cost)
            {
                buyerGold -= cost;

                if (buyerInventory.ContainsKey(itemId)) buyerInventory[itemId]++;
                else buyerInventory[itemId] = 1;

                _eventBus?.EmitLogMessage("INFO", $"Куплено: {itemId} за {cost}g");
                return true;
            }
            else
            {
                _eventBus?.EmitLogMessage("WARNING", $"Не хватает золота (нужно {cost}g)");
                return false;
            }
        }

        // Non-AI Economy Interaction (Grind)
        public void PerformWork(CharacterStats playerStats, ref int playerGold)
        {
            // Base stamina cost: 80%
            int staminaCost = Mathf.CeilToInt(playerStats.MaxStamina * 0.8f);

            if (playerStats.CurrentStamina < staminaCost)
            {
                _eventBus?.EmitLogMessage("WARNING", "Вы слишком устали, чтобы работать.");
                return;
            }

            playerStats.CurrentStamina -= staminaCost;
            int goldEarned = 10; // Fixed earning
            playerGold += goldEarned;

            _eventBus?.EmitLogMessage("INFO", $"Отработали смену (8 часов). Заработано {goldEarned}g. Потрачено {staminaCost} усталости.");

            // Advance time via TimeManager/Simulation (Stubbed here for phase 4 structure)
            // _timeManager.AdvanceHours(8);
        }
    }
}
