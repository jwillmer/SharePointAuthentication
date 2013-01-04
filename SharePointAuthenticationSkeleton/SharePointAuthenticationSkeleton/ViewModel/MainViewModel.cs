using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SharePointAuthenticationSkeleton.DataService;
using SharePointAuthenticationSkeleton.Helpers;

namespace SharePointAuthenticationSkeleton.ViewModel
{
    public class MainViewModel : ViewModelBasic
    {
        private RelayCommand LoginPopUpClosedCommand;

        public MainViewModel()
        {
            LoginPopUpClosedCommand = new RelayCommand(ExecuteLoginPopUpClosedCommand);
        }

        private async void ExecuteLoginPopUpClosedCommand()
        {
            if (SharePointAuthentication.Current == null)
                DisplayLoginPopUp();
            else await LoadData();
        }

        public override async Task OnNavigatedTo()
        {
            if (SharePointAuthentication.Current == null)
                DisplayLoginPopUp();
            else await LoadData();
        }

        private void DisplayLoginPopUp()
        {
            Messenger.Default.Send<bool>(true, "OpenLoginPopUp");
        }

        private async Task LoadData()
        {
            IsLoading = true;

            throw new NotImplementedException("Load Data From SharePoint!");
            SharePointService.GetStringFromSharePointUrl(new Uri("...."));

            IsLoading = false;
        }
    }
}