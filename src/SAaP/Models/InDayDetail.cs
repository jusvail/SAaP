namespace SAaP.Models;

public class InDayDetail
{
    public DayOfWeek DayOfWeek { get; set; }

    public string Day { get; set; }

    public DateTime DayTime
    {
        get => Convert.ToDateTime(Day);
        set => Day = value.ToString("yyyy/MM/dd");
    }

    public int Volume { get; set; }

    public double YesterdaysEnding { get; set; }

    public double Opening { get; set; }

    public double High { get; set; }

    public double Low { get; set; }

    public double Ending { get; set; }

    public double Zd { get; set; }

    public double Op { get; set; }

    public double Zf { get; set; }

    public bool IsTradingDay => Volume > 0;
}