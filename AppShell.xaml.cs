using Microsoft.Maui.Controls;
using Rentrey.Maui;
using System.Collections.Generic;

namespace Rentrey
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register a route for the PropertyPage
            Routing.RegisterRoute("PropertyPage", typeof(PropertyPage));

            // Register a route for the LoginPage
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        }
    }
}