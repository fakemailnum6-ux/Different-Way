using System.Collections.Generic;

// DTOs for SQLite and JSON Serialization (No RefCounted or Godot Node circular references)

public class PlayerStateDto
{
    public int PlayerId { get; set; } // Primary Key
    public int Level { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public int CurrentDay { get; set; }
    public int CurrentHour { get; set; }
    public int CurrentMinute { get; set; }
    public int BaseSTR { get; set; }
    public int BaseDEX { get; set; }
    public int BaseEND { get; set; }
    public int BaseINT { get; set; }
    public int BaseLuck { get; set; }
    public int Karma { get; set; }
    public int MentalRes { get; set; }
    public int Charisma { get; set; }
    public int CurrentHP { get; set; }
    public int CurrentStamina { get; set; }
    public int CurrentMana { get; set; }
    public int Gold { get; set; }
}

public class InventoryItemDto
{
    public int Id { get; set; } // Primary Key
    public int PlayerId { get; set; } // Foreign Key
    public string ItemId { get; set; }
    public int Quantity { get; set; }
    public int CurrentDurability { get; set; }
}

public class WorldNodeDto
{
    public string NodeId { get; set; } // Primary Key (e.g., "zone_forest_edge")
    public string Name { get; set; }
    public string Description { get; set; }
    public string TopologyJson { get; set; } // Serialized coordinates of buildings/roads
}

public class EntityStateDto
{
    public string EntityId { get; set; } // Primary Key
    public string WorldNodeId { get; set; } // Foreign Key
    public int IsDead { get; set; } // 1 for true, 0 for false (SQLite compatibility)
    public int CurrentHP { get; set; }
}

public class QuestProgressDto
{
    public string QuestId { get; set; } // Primary Key
    public string QuestGraphJson { get; set; } // Serialized state of objective nodes
    public int Status { get; set; } // 0 = Active, 1 = Completed, -1 = Failed
}

public class DialogueCacheDto
{
    public string HashKey { get; set; } // Primary Key (NpcId + StateHash)
    public string JsonResponse { get; set; }
}
