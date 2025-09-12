using Microsoft.Maui.Controls;

namespace Rentrey
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        protected override async void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            // Check if we are navigating to the main page and if a login is required.
            if (args.Current == null && args.Source == ShellNavigationSource.ShellSectionChanged)
            {
                // This is the first navigation after app startup.
                // Navigate to the login page.
                await Shell.Current.GoToAsync("LoginPage");
            }
        }
    }
}
