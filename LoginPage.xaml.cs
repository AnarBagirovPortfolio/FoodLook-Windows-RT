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
    public sealed partial class LoginPage : Page
    {
        private readonly NavigationHelper navigationHelper;

        public LoginPage()
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

        private async void ShowMessageAsync(string message)
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = message;
            statusBar.ProgressIndicator.ProgressValue = 0;
            await statusBar.ProgressIndicator.ShowAsync();

            await Task.Delay(1000);

            await statusBar.ProgressIndicator.HideAsync();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (this.UsernameInput.Text.Trim().Equals(String.Empty) || this.PasswordInput.Password.Trim().Equals(String.Empty))
            {
                ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("EmptyUnPMessage"));

                if (this.UsernameInput.Text.Trim().Equals(String.Empty))
                {
                    this.UsernameInput.Text = String.Empty;
                }

                if (this.PasswordInput.Password.Trim().Equals(String.Empty))
                {
                    this.PasswordInput.Password = String.Empty;
                }
            }
            else
            {
                this.RegistrationButton.IsEnabled = false;
                this.LoginButton.IsEnabled = false;

                this.InputBlock.Visibility = Visibility.Collapsed;
                this.progressRing.IsActive = true;

                string Username = this.UsernameInput.Text;
                string Password = this.PasswordInput.Password;

                string LoginSuccessful = await Api.GetTokenAsync(Username, Password);                

                this.progressRing.IsActive = false;

                if (LoginSuccessful.Equals("Okay"))
                {
                    Api.Settings.Values[Api.Username] = Username;
                    Api.Settings.Values[Api.Password] = Password;

                    if (!Frame.Navigate(typeof(MainPage), "LoginPage"))
                    {
                        throw new Exception("Navigation failed");
                    }

                    Frame.BackStack.Clear();
                }
                else
                {
                    if (LoginSuccessful.Equals("ExistError"))
                    {
                        ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("WrongUnPMessage"));
                    }
                    else if (LoginSuccessful.Equals("NetworkError"))
                    {
                        ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("NetworkErrorMessage"));
                    }
                }

                this.InputBlock.Visibility = Visibility.Visible;

                this.RegistrationButton.IsEnabled = true;
                this.LoginButton.IsEnabled = true;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.FoodLookIcon.Visibility = Visibility.Collapsed;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.FoodLookIcon.Visibility = Visibility.Visible;
        }

        private async void Registration_Click(object sender, RoutedEventArgs e)
        {
            if (this.UsernameInput.Text.Trim().Equals(String.Empty) || this.PasswordInput.Password.Trim().Equals(String.Empty))
            {
                ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("EmptyUnPMessage"));

                if (this.UsernameInput.Text.Trim().Equals(String.Empty))
                {
                    this.UsernameInput.Text = String.Empty;
                }

                if (this.PasswordInput.Password.Trim().Equals(String.Empty))
                {
                    this.PasswordInput.Password = String.Empty;
                }
            }
            else
            {
                this.RegistrationButton.IsEnabled = false;
                this.LoginButton.IsEnabled = false;

                this.InputBlock.Visibility = Visibility.Collapsed;
                this.progressRing.IsActive = true;

                string Username = this.UsernameInput.Text.Trim();
                string Password = this.PasswordInput.Password.Trim();

                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.ProgressValue = 0;
                statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("RegistrationMessage");
                await statusBar.ProgressIndicator.ShowAsync();

                string RegistrationAnswer = await Api.RegisterUserAsync(Username, Password);

                if (RegistrationAnswer.Equals("Okay"))
                {
                    statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("LoginMessage");

                    string LoginSuccessful = await Api.GetTokenAsync(Username, Password);

                    this.progressRing.IsActive = false;

                    await statusBar.ProgressIndicator.HideAsync();

                    if (LoginSuccessful.Equals("Okay"))
                    {
                        Api.Settings.Values[Api.Username] = Username;
                        Api.Settings.Values[Api.Password] = Password;

                        if (!Frame.Navigate(typeof(MainPage), "LoginPage"))
                        {
                            throw new Exception("Navigation failed");
                        }

                        Frame.BackStack.Clear();
                    }
                    else
                    {
                        if (LoginSuccessful.Equals("ExistError"))
                        {
                            ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("WrongUnPMessage"));
                        }
                        else if (LoginSuccessful.Equals("NetworkError"))
                        {
                            ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("NetworkErrorMessage"));
                        }
                    }
                }
                else
                {
                    if (RegistrationAnswer.Equals("ExistError"))
                    {
                        ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("UsernameExistsMessage"));
                    }
                    else if (RegistrationAnswer.Equals("NetworkError"))
                    {
                        ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("NetworkErrorMessage"));
                    }

                    this.progressRing.IsActive = false;
                }

                this.InputBlock.Visibility = Visibility.Visible;

                this.RegistrationButton.IsEnabled = true;
                this.LoginButton.IsEnabled = true;
            }
        }
    }
}
