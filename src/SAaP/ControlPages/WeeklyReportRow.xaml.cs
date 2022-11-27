using Mapster;
using SAaP.Models;

namespace SAaP.ControlPages;

public sealed partial class WeeklyReportRow
{
    private readonly WeeklyReportSummary _reportSummary = new();

    public WeeklyReportSummary ReportSummary
    {
        get => _reportSummary;
        set
        {
            value.Adapt(_reportSummary);
            value.Monday.Adapt(_reportSummary.Monday);
        } 
    }

    public WeeklyReportRow()
    {
        this.InitializeComponent();
    }
}