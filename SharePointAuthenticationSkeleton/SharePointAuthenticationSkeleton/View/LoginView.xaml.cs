using GalaSoft.MvvmLight.Messaging;
using SharePointAuthenticationSkeleton.ViewModel;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace SharePointAuthenticationSkeleton.View
{
    public sealed partial class LoginView : Page
    {
        public LoginView()
        {
            this.InitializeComponent();

            Messenger.Default.Register<bool>(this, "ClosePopUp", ClosePopUp);
            ((LoginViewModel)this.DataContext).OnNavigatedToCommand.Execute(null);
        }

        private void ClosePopUp(bool close)
        {
            if (Parent is Popup && close)
            {
                ((Popup)Parent).IsOpen = false;
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                ((LoginViewModel)this.DataContext).LoginCommand.Execute(null);
            }
        }
    }
}
