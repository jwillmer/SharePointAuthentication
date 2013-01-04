using GalaSoft.MvvmLight.Messaging;
using SharePointAuthenticationSkeleton.Common;
using SharePointAuthenticationSkeleton.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace SharePointAuthenticationSkeleton.View
{
    partial class MainView : LayoutAwarePage
    {
        private Popup loginPopUp;

        public MainView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Messenger.Default.Register<bool>(this, "OpenLoginPopUp", OpenLoginPopUp);
            ((MainViewModel)this.DataContext).OnNavigatedToCommand.Execute(null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Messenger.Default.Unregister<bool>(this, "OpenLoginPopUp", OpenLoginPopUp);
        }

        #region Create login PopUp

        private void OpenLoginPopUp(bool open)
        {
            if (!open)
            {
                if (loginPopUp != null)
                    loginPopUp.IsOpen = false;

                return;
            }


            var windowBounds = Window.Current.Bounds;

            loginPopUp = new Popup()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 500,
                Height = 310,
                IsLightDismissEnabled = false,
                IsOpen = true
            };

            loginPopUp.Child = new LoginView();

            loginPopUp.SetValue(Popup.HorizontalOffsetProperty, 600);
            loginPopUp.SetValue(Popup.VerticalOffsetProperty, 300);

            loginPopUp.Closed += _loginPopup_Closed;
        }

        void _loginPopup_Closed(object sender, object e)
        {
            ((MainViewModel)this.DataContext).OnNavigatedToCommand.Execute(null);
        }

        #endregion
    }
}
