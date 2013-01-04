using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SharePointAuthenticationSkeleton.ViewModel
{
    public abstract class ViewModelBasic : ViewModelBase
    {
        public RelayCommand OnNavigatedToCommand { get; private set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; RaisePropertyChanged("IsLoading"); }
        }

        public ViewModelBasic()
        {
            OnNavigatedToCommand = new RelayCommand(onNavigatedTo);   
        }

        private void onNavigatedTo()
        {
            OnNavigatedTo();
        }

        public virtual async Task OnNavigatedTo()
        { }

        public void Navigate(Type sourcePageType)
        {
            ((Frame)Window.Current.Content).Navigate(sourcePageType);
        }

        public void Navigate(Type sourcePageType, object parameter)
        {
            ((Frame)Window.Current.Content).Navigate(sourcePageType, parameter);
        }

        public void GoBack()
        {
            ((Frame)Window.Current.Content).GoBack();
        }
    }
}
