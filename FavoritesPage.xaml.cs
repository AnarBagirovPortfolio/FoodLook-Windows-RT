using FoodLook_2.Common;
using FoodLook_2.Data;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента пустой страницы см. по адресу http://go.microsoft.com/fwlink/?LinkID=390556

namespace FoodLook_2
{
    public sealed partial class FavoritesPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        private bool isIncrementalLoadingStarted = false;
        private Object IncrementalLoadingHelper = new object();

        public FavoritesPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public static async Task ShowErrorAsync()
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage");
            statusBar.ProgressIndicator.ProgressValue = 0;

            await statusBar.ProgressIndicator.ShowAsync();

            await Task.Delay(2000);

            await statusBar.ProgressIndicator.HideAsync();
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.FavoritePagePivot.IsEnabled = false;

            if (!Frame.CanGoForward)
            {
                if (e.NavigationParameter.Equals(MainPage.RestaurantsPivotItemName))
                {
                    this.FavoritePagePivot.SelectedIndex = 0;
                }
                else if (e.NavigationParameter.Equals(MainPage.CoursesPivotItemName))
                {
                    this.FavoritePagePivot.SelectedIndex = 1;
                }
            }            

            this.FavoritePagePivot.IsEnabled = true;
        }

        private async void RestaurantsLoaded(object sender, RoutedEventArgs e)
        {
            this.FavoriteRestaurantsProgressRing.IsActive = true;

            var Group = await MainPageSource.GetGroupAsync(MainPage.FavoriteRestaurantsPivotItemName);

            this.FavoriteRestaurantsProgressRing.IsActive = false;

            if (Group == null)
            {
                this.FavoriteRestaurantsErrorMessageTextBlock.Text = ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage");
                this.FavoriteRestaurantsErrorMessageTextBlock.Visibility = Visibility.Visible;
                await ShowErrorAsync();
            }
            else
            {
                this.DefaultViewModel[MainPage.FavoriteRestaurantsPivotItemName] = Group;

                if (Group.NextPageUri == null)
                {
                    this.FavoriteRestaurantsScroll.ViewChanged -= ScrollViewer_ViewChanged;
                }

                if (Group.Items.Count.Equals(0))
                {
                    this.FavoriteRestaurantsErrorMessageTextBlock.Text = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyListErrorMessage");
                    this.FavoriteRestaurantsErrorMessageTextBlock.Visibility = Visibility.Visible;
                }
            }  
        }

        private async void CoursesLoaded(object sender, RoutedEventArgs e)
        {
            this.FavoriteCoursesProgressRing.IsActive = true;

            var Group = await MainPageSource.GetGroupAsync(MainPage.FavoriteCoursesPivotItemName);

            this.FavoriteCoursesProgressRing.IsActive = false;

            if (Group == null)
            {
                this.FavoriteCoursesErrorMessageTextBlock.Text = ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage");
                this.FavoriteCoursesErrorMessageTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                this.DefaultViewModel[MainPage.FavoriteCoursesPivotItemName] = Group;

                if (Group.NextPageUri == null)
                {
                    this.FavoriteCoursesScroll.ViewChanged -= ScrollViewer_ViewChanged;
                }

                if (Group.Items.Count.Equals(0))
                {
                    this.FavoriteCoursesErrorMessageTextBlock.Text = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyListErrorMessage");
                    this.FavoriteCoursesErrorMessageTextBlock.Visibility = Visibility.Visible;
                }
            } 
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Сохраните здесь уникальное состояние страницы.
        }

        #region Регистрация NavigationHelper

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);

            if (Frame.BackStack.Count.Equals(0))
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        #endregion

        private void RestaurantClick(object sender, ItemClickEventArgs e)
        {
            var Id = ((Item)e.ClickedItem).Id;
            if (!Frame.Navigate(typeof(RestaurantPage), Id))
            {
                throw new Exception();
            }
        }

        private void CoursesClick(object sender, ItemClickEventArgs e)
        {
            var Id = ((Item)e.ClickedItem).Id;
            if (!Frame.Navigate(typeof(CoursePage), Id))
            {
                throw new Exception();
            }
        }

        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                var Sender = sender as ScrollViewer;
                if (Sender.ScrollableHeight - Sender.VerticalOffset == 0)
                {
                    lock (IncrementalLoadingHelper)
                    {
                        if (isIncrementalLoadingStarted)
                        {
                            return;
                        }
                        else
                        {
                            isIncrementalLoadingStarted = true;
                        }
                    }

                    var GroupName = Sender.Tag.ToString();

                    var ThisGroup = this.DefaultViewModel[GroupName] as Group;

                    if (ThisGroup.NextPageUri != null)
                    {
                        var statusBar = StatusBar.GetForCurrentView();
                        statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("DownloadingMessage");
                        statusBar.ProgressIndicator.ProgressValue = null;
                        await statusBar.ProgressIndicator.ShowAsync();

                        bool isSuccessful = await MainPageSource.LoadMoreItemsAsync(GroupName);

                        await statusBar.ProgressIndicator.HideAsync();

                        if (isSuccessful)
                        {
                            var Group = await MainPageSource.GetGroupAsync(GroupName);
                            if (Group.NextPageUri == null)
                            {
                                if (GroupName == MainPage.FavoriteRestaurantsPivotItemName)
                                {
                                    this.FavoriteRestaurantsScroll.ViewChanged -= ScrollViewer_ViewChanged;
                                }
                                else if (GroupName == MainPage.FavoriteCoursesPivotItemName)
                                {
                                    this.FavoriteCoursesScroll.ViewChanged -= ScrollViewer_ViewChanged;
                                }
                            }
                        }
                        else
                        {
                            await ShowErrorAsync();
                        }
                    }
                    else
                    {
                        if (GroupName == MainPage.FavoriteRestaurantsPivotItemName)
                        {
                            this.FavoriteRestaurantsScroll.ViewChanged -= ScrollViewer_ViewChanged;
                        }
                        else if (GroupName == MainPage.FavoriteCoursesPivotItemName)
                        {
                            this.FavoriteCoursesScroll.ViewChanged -= ScrollViewer_ViewChanged;
                        }
                    }

                    lock (IncrementalLoadingHelper)
                    {
                        isIncrementalLoadingStarted = false;
                    }
                }
            }
        }
    }
}
