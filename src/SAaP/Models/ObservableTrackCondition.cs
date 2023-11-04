using CommunityToolkit.Mvvm.ComponentModel;
using Mapster;
using SAaP.Core.Models.DB;

namespace SAaP.Models;

public class ObservableTrackCondition : ObservableRecipient
{
	public static readonly int DefaultTrackIndex = -1;
	private int _trackIndex = DefaultTrackIndex;
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
				if (!SelectedTrackIndex.Contains(_trackIndex))
				{
					SelectedTrackIndex.Add(_trackIndex);
				}
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

	public static readonly List<int> SelectedTrackIndex = new();

	public void Clear()
	{
		TrackIndex = DefaultTrackIndex;
		TrackName = string.Empty;
		TrackContent = string.Empty;
		TrackSummary = string.Empty;
		IsValid = false;
		IsChecked = false;
		SelectedTrackIndex.Clear();
	}

	public ObservableTrackCondition HardCopyNew()
	{
		return this.Adapt<ObservableTrackCondition>();
	}
}