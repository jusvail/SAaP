using System;
using System.Text;

namespace SAaP.Core.Models.Monitor
{
    public class MonitorNotification
    {
        public string CodeName { get; set; }

        public string CompanyName { get; set; }

        public DateTimeOffset FullTime { get; set; }

        public double Price { get; set; }

        public DealDirection Direction { get; set; }

        public double SuccessPercent { get; set; }

        public double Positions { get; set; }

        public double ExpectedProfit { get; set; }

        public double Profit { get; set; }

        public int HoldTime { get; set; }

        public string Message { get; set; }

        public int SubmittedByMode { get; set; }

        public static MonitorNotification SystemNotification(string message)
        {
            return new MonitorNotification
            {
                CompanyName = "来自系统的消息:",
                FullTime = DateTimeOffset.Now,
                Message = message
            };
        }

        public override string ToString()
        {
            return new StringBuilder(FullTime.ToString("[yyyy/MM/dd HH:mm:ss]->"))
                .Append(CodeName)
                .Append("(")
                .Append(CompanyName)
                .Append(") ")
                .Append(Direction)
                .Append("价： ")
                .Append(Price)
                .Append("  ")
                .Append(BuyMode.ModeDetails[SubmittedByMode])
                .Append("  ")
                .Append("其他信息: ")
                .Append(Message)
                .ToString();
        }
    }
}