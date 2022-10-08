using CommunityToolkit.Mvvm.ComponentModel;
using System.Text;

namespace SAaP.Models;

public class ObservableTaskDetail : ObservableRecipient
{
    private bool _isTaskNotActive = true;
    private string _execStatus = "准备就绪。";
    private string _execProgress;

    public List<ObservableTrackData> TrackDatas { get; set; } = new();

    public string TaskName { get; set; }

    public string TaskDetail
    {
        get
        {
            if (TrackDatas == null)
            {
                return string.Empty;
            }

            var s = new StringBuilder();
            foreach (var trackData in TrackDatas)
            {
                s.Append(trackData.TrackName).Append(" | ");
            }

            if (s.Length > 0)
            {
                s.Remove(s.Length - 2, 2);
            }

            return s.ToString();
        }
    }

    public bool IsTaskNotActive
    {
        get => _isTaskNotActive;
        set => SetProperty(ref _isTaskNotActive, value);
    }

    public string ExecStatus
    {
        get => _execStatus;
        set => SetProperty(ref _execStatus, value);
    }

    public string ExecProgress
    {
        get => _execProgress;
        set => SetProperty(ref _execProgress, value);
    }
}