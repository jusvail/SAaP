using System.Windows.Input;
using Mapster;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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

    public ICommand ConfirmCommand { get; set; }

    public AddToTradeListDialog()
    {
        InvestDetail = App.GetService<ObservableInvestDetail>();
        this.InitializeComponent();
    }

    private void Confirm_OnClick(object sender, RoutedEventArgs e)
    {
        ConfirmCommand?.Execute(InvestDetail);
    }

    private void TextBox_OnGettingFocus(UIElement sender, GettingFocusEventArgs args)
    {
        TextBox tb = args.OriginalSource as TextBox;
        tb.SelectAll();
        tb.PreviewKeyDown -= OnPreviewMouseDown;
    }

    void OnPreviewMouseDown(object sender, KeyRoutedEventArgs e)
    {
        TextBox tb = e.OriginalSource as TextBox;
        tb.Focus(FocusState.Keyboard);

        e.Handled = true;
    }

    private void TextBox_OnLosingFocus(UIElement sender, LosingFocusEventArgs args)
    {
        TextBox tb = args.OriginalSource as TextBox;
        tb.PreviewKeyDown += OnPreviewMouseDown;
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        TextBox tb = e.OriginalSource as TextBox;
        tb.PreviewKeyDown += OnPreviewMouseDown;
    }
}