using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAaP.Contracts.Services;
using SAaP.Core.Services;
using SAaP.Views;

namespace SAaP.Services
{
    public class ActivationService : IActivationService
    {
        private UIElement _main;

        public async Task ActivateAsync(object activationArgs)
        {
            // Execute tasks before activation.
            await InitializeAsync();

            // Set the MainWindow Content.
            if (App.MainWindow.Content == null)
            {
                _main = App.GetService<ShellPage>();
                App.MainWindow.Content = _main ?? new Frame();
            }

            App.MainWindow.Activate();
        }

        private async Task InitializeAsync()
        {
            // folder tree creation
            await Worker.EnsureWorkSpaceFolderTreeIntegrityAsync();
            // python script execution
            //await Pyer.RunPythonScript(Pyer.TdxReader, "C:/devEnv/Tools/TDX", Worker.PyDataPath, "600657");
        }
    }
}
