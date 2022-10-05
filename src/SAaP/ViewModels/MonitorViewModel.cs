using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SAaP.ViewModels;

public class MonitorViewModel : ObservableRecipient
{
    public MonitorViewModel()
    {
        AddToMonitorCommand = new RelayCommand<object>(AddToMonitor);
    }

    public IRelayCommand<object> AddToMonitorCommand { get; }

    private void AddToMonitor(object obj)
    {

    }

}