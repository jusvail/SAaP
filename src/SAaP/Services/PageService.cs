#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using SAaP.Contracts.Enum;
using SAaP.Contracts.Services;
using SAaP.ViewModels;
using SAaP.Views;

namespace SAaP.Services;

public class PageService : IPageService
{
	private readonly Dictionary<string, InstanceType> _instanceTypes = new();
	private readonly Dictionary<string, Type>         _pages         = new();

	public PageService()
	{
		Configure<MainViewModel, MainPage>(InstanceType.Single);
		Configure<MonitorViewModel, MonitorPage>(InstanceType.Single);
		Configure<SettingsViewModel, SettingsPage>(InstanceType.Single);
		Configure<InvestLogViewModel, InvestLogPage>(InstanceType.Single);
		Configure<AnalyzeDetailViewModel, AnalyzeDetailPage>(InstanceType.Multi);
		Configure<SimulatePageViewModel, SimulatePage>(InstanceType.Multi);
		Configure<StatisticsViewModel, StatisticsPage>(InstanceType.Single);
	}

	public Type GetPageType(string key)
	{
		Type? pageType;
		lock (_pages)
		{
			if (!_pages.TryGetValue(key, out pageType))
				throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
		}

		return pageType;
	}

	public InstanceType GetPageInstanceType(string key)
	{
		InstanceType type;
		lock (_instanceTypes)
		{
			if (!_instanceTypes.TryGetValue(key, out type))
				throw new ArgumentException($"InstanceType not found: {key}. Did you forget to call PageService.Configure?");
		}

		return type;
	}

	private void Configure<TVm, TV>(InstanceType tp)
		where TVm : ObservableObject
		where TV : Page
	{
		lock (_pages)
		{
			var key = typeof(TVm).FullName!;
			if (_pages.ContainsKey(key)) throw new ArgumentException($"The key {key} is already configured in PageService");

			var type = typeof(TV);
			if (_pages.Any(p => p.Value == type))
				throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");

			_pages.Add(key, type);

			lock (_instanceTypes)
			{
				_instanceTypes.Add(key, tp);
			}
		}
	}
}