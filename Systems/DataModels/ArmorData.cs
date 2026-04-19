using System.Collections.Generic;

public class ArmorData : IBaseData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Armor { get; set; }
    public int CurrentDurability { get; set; }
    public int MaxDurability { get; set; }
    public float Weight { get; set; }
    public int Price { get; set; }
    public List<string> Traits { get; set; } = new List<string>();
}
