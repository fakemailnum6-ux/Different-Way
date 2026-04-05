namespace DifferentWay.Systems;

public class StatManager
{
    public int STR { get; set; }
    public int DEX { get; set; }
    public int END { get; set; }
    public int INT { get; set; }
    public int Luck { get; set; }
    public int Charisma { get; set; }

    public int HP => (END * 5) + (STR * 2);
    public int Stamina => (END * 5) + (DEX * 2);
    public int Mana => (INT * 5);
}
