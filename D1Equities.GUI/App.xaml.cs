using D1Equities.GUI.Model;
using D1Equities.GUI.View;
using D1Equities.GUI.ViewModel;
using D1Equities.Sim;
using dotenv.net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;


namespace D1Equities.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //create command to get/set flag
        public static MarketSimulator Simulator { get; set; }

        public bool isAppShuttingDown { get; set; }

        protected async void ApplicationStart(object sender, StartupEventArgs e)

        {
            DotEnv.Load();
            Simulator = new MarketSimulator();
            await Simulator.InitAsync();
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
                        var user = (UserModel)Application.Current.Properties["User"];
                        var mainVM = new MainViewModel(user);
                        var mainView = new MainView(mainVM);
                        mainView.Show();
                        loginView.Close();
                    }
                }
                else
                {
                    Simulator.Dispose();
                }
            };

            loginView.IsVisibleChanged += handler;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            isAppShuttingDown = true;

            try
            {
                var user = Application.Current.Properties["User"] as UserModel;
                if (user != null && user.Portfolio != null)
                {
                    try
                    {
                        user.Portfolio.Save();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                            Directory.CreateDirectory(logDir);
                            var logFile = Path.Combine(logDir, "Save_error.log");
                            var line = $"{DateTime.Now}: Error saving portfolio on exit: {ex}\n";
                            File.AppendAllText(logFile, line);
                        }
                        catch
                        {
                            /* swallows*/
                        }
                    }
                }
            }
            catch
            {

            }
        }
    }
}
