using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAaP.Contracts.Services;

namespace SAaP.Services
{
    public class ActivationService : IActivationService
    {
        private UIElement _main = null;

        public async Task ActivateAsync(object activationArgs)
        {
            // Execute tasks before activation.
            await InitializeAsync();

            // Set the MainWindow Content.
            if (App.MainWindow.Content == null)
            {
                _main = App.GetService<MainWindow>();
                App.MainWindow.Content = _main ?? new Frame();
            }

            App.MainWindow.Activate();
        }

        private async Task InitializeAsync()
        {
            // TODO do something
            await Task.CompletedTask;
        }
    }
}
