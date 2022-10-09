using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text;
using Microsoft.UI.Xaml;
using SAaP.Core.Models.DB;

namespace SAaP.Models;

public class ObservableTaskDetail : ObservableRecipient
{
    private bool _isCompleted = true;
    private string _execStatus = "准备就绪。";
    private string _execProgress;
    private bool _isCancelled = true;
    private string _execButtonText = ButtonText[0];

    private static readonly string[] ButtonText = { "执行", "取消" };

    public List<ObservableTrackCondition> TrackConditions { get; set; } = new();

    public string TaskName { get; set; }

    public string TaskDetail
    {
        get
        {
            if (TrackConditions == null)
            {
                return string.Empty;
            }

            var s = new StringBuilder();

            foreach (var trackData in TrackConditions)
            {
                s.Append(trackData.TrackName).Append(" | ");
            }

            if (s.Length > 0)
            {
                s.Remove(s.Length - 3, 3);
            }

            return s.ToString();
        }
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
    public bool IsCompleted
    {
        get => _isCompleted;
        set => SetProperty(ref _isCompleted, value);
    }

    public bool IsCancelled
    {
        get => _isCancelled;
        set
        {
            SetProperty(ref _isCancelled, value);
            ExecButtonText = _isCancelled ? ButtonText[0] : ButtonText[1];
            ProgressRingVisibility = IsCancelled ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public Visibility ProgressRingVisibility
    {
        get => _progressRingVisibility;
        set => SetProperty(ref _progressRingVisibility, value);
    }


    public string ExecButtonText
    {
        get => _execButtonText;
        set => SetProperty(ref _execButtonText, value);
    }

    public double ProgressBarValue
    {
        get => _progressBarValue;
        set => SetProperty(ref _progressBarValue, value);
    }

    public ObservableCollection<Stock> FilteredStock { get; set; } = new();

    public IAsyncRelayCommand StartTaskCommand { get; }

    public event EventHandler<TaskStartEventArgs> TaskStartEventHandler;

    private CancellationTokenSource _cts;
    private Visibility _progressRingVisibility;
    private double _progressBarValue;

    public ObservableTaskDetail()
    {
        StartTaskCommand = new AsyncRelayCommand(StartTask);
    }

    private void CancelTask()
    {
        _cts?.Cancel();
    }

    private async Task StartTask()
    {
        if (IsCancelled)
        {
            CancelTask();
            return;
        }

        var cts = new CancellationTokenSource();

        _cts = cts;

        var mainExec = new Task(ExecTask, cts.Token);

        try
        {
            mainExec.Start();
            await mainExec.WaitAsync(cts.Token);
        }
        catch (Exception)
        {
            //ignore
        }
    }

    public void OnTaskComplete()
    {
        IsCancelled = true;
        _cts.Dispose();
        _cts = null;
    }

    public void OnTaskCancelled()
    {
        IsCompleted = true;
        _cts.Dispose();
        _cts = null;
    }

    protected void ExecTask()
    {
        TaskStartEventHandler?.Invoke(this,
            new TaskStartEventArgs
              {
                  TitleBarMessage = "开始了！！"
                  ,
                  CancellationToken = _cts.Token
              });
    }
}