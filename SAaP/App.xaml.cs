using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SAaP.Constant;
using SAaP.Contracts.Services;
using SAaP.Extensions;
using SAaP.Services;
using SAaP.Views;

namespace SAaP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App
    {

        private static readonly IHost _host = Host.CreateDefaultBuilder().ConfigureServices(
            (context, services) =>
            {
                services.AddSingleton<IActivationService, ActivationService>();

                services.AddTransient<MainFrame>();
            }
        ).Build();

        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService<T>();
        }

        public static Window MainWindow { get; set; } = new() {Title = PjConstant.AppTitle.GetLocalized() };

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var activationService = GetService<IActivationService>();
            activationService.ActivateAsync(args);
        }

    }
}
