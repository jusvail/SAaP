using CommunityToolkit.Mvvm.ComponentModel;
using Mapster;
using SAaP.Core.Models.DB;

namespace SAaP.Models;

public class ObservableTrackCondition : ObservableRecipient
{
    private int _trackIndex = -1;
    private string _trackName;
    private string _trackContent;
    private string _trackSummary;
    private TrackType _trackType;
    private bool _isValid;
    private bool _isChecked;

    public int TrackIndex
    {
        get => _trackIndex;
        set => SetProperty(ref _trackIndex, value);
    }

    public string TrackName
    {
        get => _trackName;
        set => SetProperty(ref _trackName, value);
    }

    public string TrackContent
    {
        get => _trackContent;
        set => SetProperty(ref _trackContent, value);
    }

    public string TrackSummary
    {
        get => _trackSummary;
        set => SetProperty(ref _trackSummary, value);
    }

    public TrackType TrackType
    {
        get => _trackType;
        set => SetProperty(ref _trackType, value);
    }

    public bool IsValid
    {
        get => _isValid;
        set => SetProperty(ref _isValid, value);
    }

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            SetProperty(ref _isChecked, value);
            if (_isChecked)
            {
                SelectedTrackIndex.Add(_trackIndex);
            }
            else
            {
                if (SelectedTrackIndex.Contains(_trackIndex))
                {
                    SelectedTrackIndex.Remove(_trackIndex);
                }
            }
        }
    }

    public static List<int> SelectedTrackIndex = new();

    public void Clear()
    {
        _trackIndex = -1;
        _trackName = string.Empty;
        _trackContent = string.Empty;
        _trackSummary = string.Empty;
        _isValid = false;
        IsChecked = false;
    }

    public ObservableTrackCondition HardCopyNew()
    {
        return this.Adapt<ObservableTrackCondition>();
    }
}