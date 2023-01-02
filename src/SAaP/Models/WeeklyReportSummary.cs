using CommunityToolkit.Mvvm.ComponentModel;

namespace SAaP.Models;

public class WeeklyReportSummary : ObservableRecipient
{
    private string _companyName;

    public WeeklyReport Monday { get; set; } = new();

    public WeeklyReport Tuesday { get; set; } = new();

    public WeeklyReport Wednesday { get; set; } = new();

    public WeeklyReport Thursday { get; set; } = new();

    public WeeklyReport Friday { get; set; } = new();

    public string CompanyName
    {
        get => _companyName;
        set => SetProperty(ref _companyName, value);
    }

    public void Clear()
    {
        Monday = new WeeklyReport();
        Tuesday = new WeeklyReport();
        Wednesday = new WeeklyReport();
        Thursday = new WeeklyReport();
        Friday = new WeeklyReport();
    }
}