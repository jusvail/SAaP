using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace SAaP.Models;

[JsonObject(MemberSerialization.OptOut)]
public partial class ObservableStatisticsDetail  : ObservableTaskDetail
{
	public ObservableStatisticsDetail() :base()
	{
		YAxis = new List<string>()
		{
			"期望收益","最大回撤(达到期望收益)","最大回撤时天数(达到期望收益)",
		};
	}

	[ObservableProperty] private double _expectedProfit;
	[ObservableProperty] private double _pullUpBeforeStart;
	[ObservableProperty] private double _pullUpBeforeEnd;
	[ObservableProperty] private int    _selectedYAxis;
	[ObservableProperty] private int    _stepLength;
	
	[ObservableProperty] private DateTimeOffset _endDate;
	[ObservableProperty] private DateTimeOffset _startDate;
	  
	public new string TaskDetail { get; set; }
	  
	public readonly List<string> YAxis;

}