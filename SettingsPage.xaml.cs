using FoodLook_2.Common;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента пустой страницы см. по адресу http://go.microsoft.com/fwlink/?LinkID=390556

namespace FoodLook_2
{
    public sealed partial class SettingsPage : Page
    {
        private readonly NavigationHelper navigationHelper;

        public SettingsPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Создайте соответствующую модель данных для своей проблемной области, чтобы заменить ими данные-пример.
            string CurrentCulture = CultureInfo.CurrentCulture.Name;

            if (CurrentCulture.Contains("ru"))
            {
                this.SelectLanguageComboBox.SelectedIndex = 1;
            }
            else
            {
                this.SelectLanguageComboBox.SelectedIndex = 0;
            }
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

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            var Settings = ApplicationData.Current.RoamingSettings;
            Settings.Values.Clear();

            if (!Frame.Navigate(typeof(LoginPage), null))
            {
                throw new Exception();
            }

            Frame.BackStack.Clear();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(ChangingPasswordPage), null))
            {
                throw new Exception();
            }
        }
    }
}
