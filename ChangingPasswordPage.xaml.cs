using FoodLook_2.Common;
using FoodLook_2.DataModel;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FoodLook_2
{
    public sealed partial class ChangingPasswordPage : Page
    {
        private readonly NavigationHelper navigationHelper;

        public ChangingPasswordPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        #region Регистрация NavigationHelper

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.AcceptButton.IsEnabled = false;

            string OldPassword = this.OldPassword.Password;
            string NewPassword = this.NewPassword.Password;

            if (OldPassword.Equals(String.Empty) || NewPassword.Equals(String.Empty))
            {
                ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("EmptyErrorMessage"));
            }
            else
            {
                if (OldPassword.Equals(Api.Settings.Values[Api.Password].ToString()))
                {
                    this.ChangingGrid.Visibility = Visibility.Collapsed;

                    this.progressRing.IsActive = true;

                    bool Successful = await Api.ChangePasswordAsync(OldPassword, NewPassword);

                    this.progressRing.IsActive = false;

                    if (Successful)
                    {
                        Api.Settings.Values[Api.Password] = NewPassword;

                        ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("SuccessfulChanging"));

                        Frame.GoBack();

                        this.OldPassword.Password = String.Empty;
                        this.NewPassword.Password = String.Empty;
                    }
                    else
                    {
                        ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
                    }
                }
                else
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("IncorrectPassword"));
                }
            }

            this.AcceptButton.IsEnabled = true;
        }

        private async void ShowMessageAsync(string message)
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = message;
            statusBar.ProgressIndicator.ProgressValue = 0;
            await statusBar.ProgressIndicator.ShowAsync();

            await Task.Delay(2000);

            await statusBar.ProgressIndicator.HideAsync();
        }
    }
}
