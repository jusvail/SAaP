namespace SAaP.Analyst.Models;

public class Report
{
    public bool ErrorHappened { get; set; }

    public string CompanyName { get; set; }

    public string CodeName { get; set; }

    public string CurrentStep { get; set; }

    public string Step1 { get; set; }

    public string Step2 { get; set; }

    public string Step2Progress { get; set; }

    public string BuyAt { get; set; }

    public string BuyPrice { get; set; }

    public string Step3 { get; set; }

    public string SellAt { get; set; }

    public string SellPrice { get; set; }

    public string HoldingDays { get; set; }

    public string Profit { get; set; }

    public string Step4 { get; set; }

    public static readonly Report Blank = new();

    public static Report ErrorWith(string codeName, string companyName,string message ="")
    {
        return new Report
        {
            CodeName = codeName,
            CompanyName = companyName,
            ErrorHappened = true,
            CurrentStep = message,
        };
    }
}