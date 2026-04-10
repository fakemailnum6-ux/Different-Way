using System.Collections.Generic;

public class ConsumableData : IBaseData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Effect { get; set; }
    public float Weight { get; set; }
    public int Price { get; set; }
    public List<string> Traits { get; set; } = new List<string>();
}
