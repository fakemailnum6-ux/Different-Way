using Godot;

public partial class TimeManager : Node
{
    public int Year { get; private set; } = 1;
    public int Month { get; private set; } = 1;
    public int Day { get; private set; } = 1;
    public int Quarter { get; private set; } = 0; // 0=Morning, 1=Day, 2=Evening, 3=Night

    public override void _Ready()
    {
        GD.Print("[TimeManager] Initialized.");
    }

    public void AdvanceTime(int quartersToAdd)
    {
        Quarter += quartersToAdd;
        while (Quarter >= 4)
        {
            Quarter -= 4;
            Day++;
            if (Day > 30)
            {
                Day = 1;
                Month++;
                if (Month > 12)
                {
                    Month = 1;
                    Year++;
                }
            }
        }
    }

    public string GetTimeString()
    {
        string timeStr = "";
        switch (Quarter)
        {
            case 0: timeStr = "Morning"; break;
            case 1: timeStr = "Day"; break;
            case 2: timeStr = "Evening"; break;
            case 3: timeStr = "Night"; break;
        }
        return $"Year {Year}, Month {Month}, Day {Day} - {timeStr}";
    }
}
