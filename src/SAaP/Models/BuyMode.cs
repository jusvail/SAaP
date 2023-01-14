using CommunityToolkit.Mvvm.ComponentModel;

namespace SAaP.Models
{
    public class BuyMode
    {
        public static readonly int NLowRisk = 0;
        public static readonly int NHighRisk = 1;
        public static readonly int AHighRisk = 2;
        public static readonly int WMiddleRisk = 3;

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
