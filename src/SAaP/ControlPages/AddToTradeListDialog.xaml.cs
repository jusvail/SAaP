using CommunityToolkit.Mvvm.Input;
using Mapster;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SAaP.Helper;
using SAaP.Models;

namespace SAaP.ControlPages;

/// <summary>
/// trade detail dialog/fly out
/// </summary>
public sealed partial class AddToTradeListDialog
{
    private readonly ObservableInvestDetail _investDetail = new();

    public ObservableInvestDetail InvestDetail
    {
        get => _investDetail;
        set => value.Adapt(_investDetail);
    }

    public IRelayCommand<object> ConfirmCommand { get; set; }
    public IRelayCommand<object> DeleteCommand { get; set; }

    public Button Sender { get; set; }

    public AddToTradeListDialog()
    {
        InvestDetail = App.GetService<ObservableInvestDetail>();
        this.InitializeComponent();
    }

    private void Confirm_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Volume.Text) || string.IsNullOrEmpty(Price.Text))
        {
            return;
        }

        InvestDetail.Volume = Convert.ToInt32(Volume.Text);
        InvestDetail.Price = Convert.ToDouble(Price.Text);

        UiInvokeHelper.HideButtonFlyOut(Sender);
        if (ConfirmCommand == null)
        {
            InvestDetail.ConfirmCommand?.Execute(InvestDetail);
        }
        else
        {
            ConfirmCommand.Execute(InvestDetail);
        }
    }

    private void Delete_OnClick(object sender, RoutedEventArgs e)
    {
        UiInvokeHelper.HideButtonFlyOut(Sender);
        if (DeleteCommand == null)
        {
            InvestDetail.DeleteCommand?.Execute(InvestDetail);
        }
        else
        {
            DeleteCommand.Execute(InvestDetail);
        }
    }

    private void TextBox_OnGettingFocus(UIElement sender, GettingFocusEventArgs args)
    {
        var tb = args.OriginalSource as TextBox;
        if (tb == null) return;
        tb.SelectAll();
        tb.PreviewKeyDown -= OnPreviewMouseDown;
    }

    private static void OnPreviewMouseDown(object sender, KeyRoutedEventArgs e)
    {
        var tb = e.OriginalSource as TextBox;
        if (tb != null) tb.Focus(FocusState.Keyboard);

        e.Handled = true;
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        var tb = e.OriginalSource as TextBox;
        if (tb != null) tb.PreviewKeyDown += OnPreviewMouseDown;
    }
}