using Microsoft.UI.Xaml.Navigation;
using SAaP.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SAaP.Core.Models.DB;
using SAaP.Helper;

namespace SAaP.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MonitorPage
{
    public MonitorViewModel ViewModel { get; }
    
    public MonitorPage()
    {
        ViewModel = App.GetService<MonitorViewModel>();

        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        await ViewModel.InitializeSuggestData();
        await ViewModel.InitializeTrackData();
    }

    private void AddToMonitorAppBarButton_OnClick(object sender, RoutedEventArgs e)
    {
        UiInvokeHelper.TriggerButtonPressed(AddToMonitorAppBarButtonHidden);
    }

    private void CodeSelectSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
        if (string.IsNullOrEmpty(sender.Text))
        {
            CodeSelectSuggestBox.ItemsSource = null;
            return;
        }

        var splitText = sender.Text.Split(" ");
        CodeSelectSuggestBox.ItemsSource = ViewModel.GetCodeSelectSuggest(splitText);
    }

    private void CodeSelectSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is Stock stock)
        {
            ViewModel.AddToMonitor(stock);
        }
        else if (!string.IsNullOrEmpty(args.QueryText))
        {
            ViewModel.AddToMonitor(args.QueryText);
        }
        sender.Text = string.Empty;
    }

    private void CodeSelectSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        ViewModel.AddToMonitor(args.SelectedItem);
        sender.Text = string.Empty;
    }

    private void DeleteMonitor_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.DeleteMonitorItem(((FrameworkElement)e.OriginalSource).DataContext);
    }

    private void HelperAppBarButton_OnClick(object sender, RoutedEventArgs e)
    {
        FilterTextBoxTeachingTip.IsOpen = true;
    }

    private void FilterTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        ViewModel.CurrentTrackFilterCondition.IsValid = false;
    }

    private void SaveFilterConditionButton_OnClick(object sender, RoutedEventArgs e)
    {
        SaveCondition.Flyout.Hide();
    }

    private async void DeleteFilterCondition_OnClick(object sender, RoutedEventArgs e)
    {
       var dataContext = (e.OriginalSource as MenuFlyoutItem)?.DataContext;

       await ViewModel.DeleteFilterTrackData(dataContext);
    }

}