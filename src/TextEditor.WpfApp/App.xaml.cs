using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TextEditor.WpfApp.ViewModel;

namespace TextEditor.WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }

        public App()
        {
            Services = ConfigureServices();
            InitializeComponent();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<MainWindowVm>();

            return services.BuildServiceProvider();
        }
    }

}
