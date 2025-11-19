using D1Equities.GUI.Model;
using D1Equities.Sim;
using System.Security;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;

namespace D1Equities.GUI.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        //Fields

        private string _username;
        private SecureString _password;
        private string _errorMessage;
        private bool _isViewVisible = true;

        private IUserRepository userRepository;

        //Properties
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
        public SecureString Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        public bool IsViewVisible
        {
            get
            {
                return _isViewVisible;
            }
            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }
        //Commands
        public ICommand LoginCommand { get; }
        public ICommand ShowPasswordCommand { get; }

        //constructor
        public LoginViewModel()
        {
            userRepository= new JsonUserRepository();
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            
        }


        private bool CanExecuteLoginCommand(object obj)
        {
 
            bool ValidData;
            if(string.IsNullOrWhiteSpace(Username) || Username.Length < 1 || Password == null || Password.Length < 1)
            {
                ValidData = false;
            }
            else
            {
                ValidData = true;
            }
            return ValidData;
        }

        private void ExecuteLoginCommand(object obj)
        {
            var isValidUser = userRepository.AuthenticateUser(new System.Net.NetworkCredential(Username, Password));
            if (isValidUser)
            {
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(Username), null);
                var user = userRepository.GetByUsername(Username);
                user.Portfolio = Portfolio.Load(user.Id);

                if(user.Portfolio == null)
                {
                    ErrorMessage = "Couldnt load user portfolio";
                    return;
                }

                Application.Current.Properties["User"] = user;
                IsViewVisible = false;
            }
            else
            {
                ErrorMessage = "Invalid username or password";
            }
               
        }
        

    }
}
