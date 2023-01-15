using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SAaP.Core.Models.Monitor
{
    public class BuyMode
    {
        public const int NLowRisk = 0;
        public const int NHighRisk = 1;
        public const int AHighRisk = 2;
        public const int WMiddleRisk = 3;

        public static readonly List<string> ModeDetails = new()
        {
            "N型低风险", "N型高风险", "A型高风险", "W型中风险"
        };

        public static IEnumerable<ObservableBuyMode> All()
        {
            var list = new List<int> { NLowRisk, NHighRisk, AHighRisk, WMiddleRisk };

            foreach (var i in list)
            {
                yield return new ObservableBuyMode { BuyMode = i, ModeDetail = ModeDetails[i] };
            }
        }
    }

    public class ObservableBuyMode : ObservableRecipient
    {
        private int _buyMode;
        private string _modeDetail;
        private bool _isChecked;

        public int BuyMode
        {
            get => _buyMode;
            set => SetProperty(ref _buyMode, value);
        }

        public string ModeDetail
        {
            get => _modeDetail;
            set => SetProperty(ref _modeDetail, value);
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
    }
}
