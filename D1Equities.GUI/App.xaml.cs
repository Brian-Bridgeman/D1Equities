using D1Equities.GUI.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace D1Equities.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //create command to get/set flag

        public bool isAppShuttingDown { get; set; }

        protected void ApplicationStart(object sender, StartupEventArgs e)

        {
            var loginView = new LoginView();
            loginView.Show();
            //declare handler as variable so we can explicitly unsubscribe

            DependencyPropertyChangedEventHandler handler = null;

            handler = (s, ev) =>
            {
                //check to see if app is shutting down BEFORE we check the state of the loginView window
                if (!isAppShuttingDown)
                {
                    if (loginView.IsVisible == false && loginView.IsLoaded)
                    {
                        loginView.IsVisibleChanged -= handler; // Unsubscribe properly
                        var mainView = new MainView();
                        mainView.Show();
                        loginView.Close();
                    }
                }
            };

            loginView.IsVisibleChanged += handler;
        }
    }
}
