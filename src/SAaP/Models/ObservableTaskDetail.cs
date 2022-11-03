using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAaP.Core.Models.DB;

namespace SAaP.Models;

public class ObservableTaskDetail : ObservableRecipient
{
    #region class field

    private static readonly string[] ButtonText = { "执行", "取消" };

    private bool _isCompleted;
    private bool _isCancelled;
    private bool _isTaskStart;

    private double _progressBarValue;

    private string _execProgress;
    private string _execStatus = "准备就绪。";
    private string _execButtonText = ButtonText[0];

    private CancellationTokenSource _cts;
    private Visibility _progressRingVisibility = Visibility.Collapsed;
    private Visibility _resultButtonVisibility = Visibility.Collapsed;
    private string _taskName;

    public ObservableCollection<Stock> FilteredStock { get; set; } = new();

    public IAsyncRelayCommand StartTaskCommand { get; }

    public event EventHandler<TaskStartEventArgs> TaskStartEventHandler;

    public event EventHandler<NavigateToTabViewEventArgs> NavigateToTabViewEventHandler;

    #endregion

    public List<ObservableTrackCondition> TrackConditions { get; set; } = new();

    private static int _defaultTaskIndex;

    public static string DefaultTaskName
    {
        get
        {
            _defaultTaskIndex++;
            return $"默认任务 {_defaultTaskIndex}";
        }
    }

    public string TaskName
    {
        get => string.IsNullOrEmpty(_taskName) ? DefaultTaskName : _taskName;
        set => _taskName = value;
    }
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
                s.Append(trackData.TrackName).Append(" && ");
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
        set
        {
            SetProperty(ref _isCompleted, value);
            OnTaskStatusChanged();
        }
    }

    public bool IsCancelled
    {
        get => _isCancelled;
        set
        {
            SetProperty(ref _isCancelled, value);
            OnTaskStatusChanged();
        }
    }

    public Visibility ProgressRingVisibility
    {
        get => _progressRingVisibility;
        set => SetProperty(ref _progressRingVisibility, value);
    }

    public bool IsTaskStart
    {
        get => _isTaskStart;
        set
        {
            SetProperty(ref _isTaskStart, value);
            OnTaskStatusChanged();
        }
    }

    public Visibility ResultButtonVisibility
    {
        get => _resultButtonVisibility;
        set => SetProperty(ref _resultButtonVisibility, value);
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

    public IRelayCommand<TabView> ViewResultCommand { get; }

    public ObservableTaskDetail()
    {
        ViewResultCommand = new RelayCommand<TabView>(NavigateToTabView);
        StartTaskCommand = new AsyncRelayCommand(StartTask);
    }

    private void OnTaskStatusChanged()
    {
        ResultButtonVisibility = _isCompleted || _isCancelled ? Visibility.Visible : Visibility.Collapsed;

        ExecButtonText = _isTaskStart ? ButtonText[1] : ButtonText[0];
        ProgressRingVisibility = _isTaskStart ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnTaskStart()
    {
        IsCompleted = false;
        IsCancelled = false;

        IsTaskStart = true;

        FilteredStock.Clear();
    }

    private void OnTaskFinished()
    {
        _cts.Dispose();
        _cts = null;

        IsTaskStart = false;
    }

    private void CancelTask()
    {
        _cts?.Cancel();
    }

    private async Task StartTask()
    {
        if (IsTaskStart)
        {
            CancelTask();
            return;
        }

        OnTaskStart();

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
        IsCompleted = true;
        OnTaskFinished();
    }

    public void OnTaskCancelled()
    {
        IsCancelled = true;
        OnTaskFinished();
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

    protected void NavigateToTabView(TabView tabView)
    {
        NavigateToTabViewEventHandler?.Invoke(this,
            new NavigateToTabViewEventArgs
            {
                TaskName = TaskName,
                Stocks = FilteredStock,
                Target = tabView
            });
    }
}