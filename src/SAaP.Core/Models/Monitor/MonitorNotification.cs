using System;

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
    }
}