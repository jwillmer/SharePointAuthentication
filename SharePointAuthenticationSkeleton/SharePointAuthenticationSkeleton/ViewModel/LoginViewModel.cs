using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SharePointAuthenticationSkeleton.Helpers;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Windows.System.UserProfile;

namespace SharePointAuthenticationSkeleton.ViewModel
{
    public class LoginViewModel : ViewModelBasic
    {
        #region Propertys

        private readonly string PasswordVaultName;

        public RelayCommand LoginCommand { get; private set; }

        bool LoginCommandCanExecute()
        {
            return !string.IsNullOrEmpty(Username)
                && !string.IsNullOrWhiteSpace(Userpassword)
                && IsConnectedToInternet
                && !IsLoading;
        }

        private string _serverUrl;
        public string ServerUrl
        {
            get { return _serverUrl; }
            set
            {
                if (_serverUrl == value) return;
                _serverUrl = value;
                RaisePropertyChanged("ServerUrl");
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                if (_username == value) return;
                _username = value;
                RaisePropertyChanged("Username");
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private string _userpassword;
        public string Userpassword
        {
            get { return _userpassword; }
            set
            {
                if (_userpassword == value) return;
                _userpassword = value;
                RaisePropertyChanged("Userpassword");
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _saveUserCredentials = false;
        public bool SaveUserCredentials
        {
            get { return _saveUserCredentials; }
            set
            {
                if (_saveUserCredentials == value) return;
                _saveUserCredentials = value;
                RaisePropertyChanged("SaveUserCredentials");
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message == value) return;
                _message = value;
                RaisePropertyChanged("Message");
            }
        }

        private bool _isConnectedToInternet;
        public bool IsConnectedToInternet
        {
            get { return _isConnectedToInternet; }
            set
            {
                if (_isConnectedToInternet == value) return;
                _isConnectedToInternet = value;

                Message = value ? string.Empty : "Keine Internetverbindung!";
                IsLoading = false;
                LoginCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged("IsConnectedToInternet");
            }
        }

        #endregion

        public LoginViewModel()
        {
            PasswordVaultName = new ResourceLoader().GetString("PasswordVaultName");
            LoginCommand = new RelayCommand(ExecuteLoginCommand, LoginCommandCanExecute);
            ServerUrl = "http://YourSharePointServer.com";
            IsConnectedToInternet = NetworkInformation.GetInternetConnectionProfile() != null;
            InternetHelpers.InternetConnectionChanged += (sender, args) => IsConnectedToInternet = args.IsConnected;
        }

        public override async Task OnNavigatedTo()
        {
            if (!InternetHelpers.IsConnected)
                return;

            IsLoading = true;
            Message = "Suche vorhandene Anmeldedaten...";
            bool successfulAuth = false;

            try
            {
                // Try to get the user's Office 365 credentials from the password vault
                var pwVault = new PasswordVault();
                var creds = pwVault.FindAllByResource(PasswordVaultName);
                PasswordCredential credentials = creds.FirstOrDefault();
                credentials.RetrievePassword();

                Username = credentials.UserName;
                Userpassword = credentials.Password;
                Message = "Anmeldeversuch...";

                if (!credentials.Properties.ContainsKey("Url"))
                    throw new Exception("Nothing found!");

                successfulAuth = await SharePointAuthentication.Create(
                    new Uri(credentials.Properties["Url"].ToString()),
                    credentials.UserName,
                    credentials.Password,
                    false);
            }
            catch (Exception)
            {
            }

            IsLoading = false;
            Message = string.Empty;

            if (successfulAuth)
                Messenger.Default.Send<bool>(true, "ClosePopUp");

            try
            {

                IsLoading = true;
                Message = "Anmeldeversuch mit Systemkonto....";
                Username = await UserInformation.GetPrincipalNameAsync();
                Userpassword = string.Empty;

                // Try login with system user principals
                successfulAuth = await SharePointAuthentication.Create(
                     new Uri(ServerUrl),
                     Username,
                     null,
                     true);
            }
            catch (Exception)
            {
            }

            IsLoading = false;
            Message = string.Empty;
            Username = string.Empty;
            Userpassword = string.Empty;

            if (successfulAuth)
                Messenger.Default.Send<bool>(true, "ClosePopUp");
        }

        private async void ExecuteLoginCommand()
        {
            Message = string.Empty;
            bool successfulAuth = false;
            IsLoading = true;

            try
            {
                // Attempt to sign the user into SharePoint Online using Integrated Windows Auth or username + password
                successfulAuth = await SharePointAuthentication.Create(
                   new Uri(ServerUrl),
                   Username,
                   Userpassword,
                   false);
            }
            catch (Exception)
            {
            }

            IsLoading = false;

            if (!successfulAuth)
            {
                Message = "Anmeldung fehlgeschlagen!";
                return;
            }

            if (SaveUserCredentials)
            {
                var pwVault = new PasswordVault();
                IReadOnlyList<PasswordCredential> credentials = null;

                // Find existing credentials
                try
                {
                    credentials = pwVault.FindAllByResource(PasswordVaultName);

                    // Remove existing credentials
                    if (credentials != null)
                        foreach (PasswordCredential credential in credentials)
                            pwVault.Remove(credential);
                }
                catch { }

                var pwCredentials = new PasswordCredential(PasswordVaultName, Username, Userpassword);
                pwCredentials.Properties.Add(new KeyValuePair<string, object>("Url", ServerUrl));
                pwVault.Add(pwCredentials);
            }

            Messenger.Default.Send<bool>(true, "ClosePopUp");
        }
    }
}
