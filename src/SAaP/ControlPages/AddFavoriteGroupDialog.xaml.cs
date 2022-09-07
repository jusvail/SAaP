using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SAaP.ControlPages;

public sealed partial class AddFavoriteGroupDialog
{
    public AddFavoriteGroupDialog()
    {
        this.InitializeComponent();
        FavoriteListSelectSelectIndex = 0;
    }

    public AddFavoriteGroupDialog(List<string> groupNames) : this()
    {
        GroupNames = groupNames;
    }

    public List<string> GroupNames { get; set; }

    public string GroupName { get; set; }

    public bool CreateNewChecked { get; set; }
    public int FavoriteListSelectSelectIndex { get; set; }

    private void FavoriteListSelect_OnTextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
    {
        GroupName = FavoriteListSelect.SelectedItem.ToString();
    }

    private void CreateNew_OnChecked(object sender, RoutedEventArgs e)
    {
        FavoriteListSelect.IsEnabled = false;
        NewGroupName.Visibility = Visibility.Visible;
    }

    private void CreateNew_OnUnchecked(object sender, RoutedEventArgs e)
    {
        FavoriteListSelect.IsEnabled = true;
        NewGroupName.Visibility = Visibility.Collapsed;
    }
}