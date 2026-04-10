using System.Collections.Generic;

public class SkillData : IBaseData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string CostType { get; set; }
    public int CostAmount { get; set; }
    public int Cooldown { get; set; }
    public string Description { get; set; }
    public List<string> Traits { get; set; } = new List<string>();
}
