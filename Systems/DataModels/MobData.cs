using System.Collections.Generic;

public class MobData : IBaseData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int HP { get; set; }
    public int Damage { get; set; }
    public int Armor { get; set; }
    public int DEX { get; set; }
    public int STR { get; set; }
    public List<string> Traits { get; set; } = new List<string>();
    public List<string> LootTable { get; set; } = new List<string>();
}
