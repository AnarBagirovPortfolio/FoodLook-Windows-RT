using FoodLook_2.Common;
using FoodLook_2.Data;
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
    public sealed partial class MainPage : Page
    {
        public const string RestaurantsPivotItemName = "Restaurants";
        public const string PromotedRestaurantsPivotItemName = "PromotedRestaurants";
        public const string CoursesPivotItemName = "Courses";
        public const string VegetariaCoursesPivotItemName = "VegetarianCourses";
        public const string CategoriesPivotItemName = "Categories";
        public const string FavoriteRestaurantsPivotItemName = "FavoriteRestaurants";
        public const string FavoriteCoursesPivotItemName = "FavoriteCourses";

        public const int RestaurantsPivotItemIndex = 0;
        public const int CategoriesPivotItemIndex = 1;
        public const int CoursesPivotItemIndex = 2;
        public const int PromotedRestaurantsPivotItemIndex = 3;

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private bool isIncrementalLoadingStarted = false;
        private Object IncrementalLoadingHelper = new object();

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

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

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Создание соответствующей модели данных для области проблемы, чтобы заменить пример данных
            if (this.RestarauntsMessage.Visibility == Visibility.Visible) this.RestarauntsMessage.Visibility = Visibility.Collapsed;
            if (this.RefreshButton.Visibility == Visibility.Visible) this.RefreshButton.Visibility = Visibility.Collapsed;

            this.RestaurantsProgressRing.IsActive = true;
            this.MainPagePivot.IsEnabled = false;

            var FirstGroup = await MainPageSource.GetGroupAsync(RestaurantsPivotItemName);

            this.RestaurantsProgressRing.IsActive = false;
            this.MainPagePivot.IsEnabled = true; 

            if (FirstGroup == null)
            {
                this.RestarauntsMessage.Text = ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage");
                this.RestarauntsMessage.Visibility = Visibility.Visible;

                this.RefreshButton.Visibility = Visibility.Visible;

                this.RestaurantsScroll.ViewChanged -= Scroll_ViewChanged;

                await ShowErrorAsync();
            }
            else
            {
                if (FirstGroup.Items.Count.Equals(0))
                {
                    this.RestarauntsMessage.Text = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyListErrorMessage");
                    this.RestarauntsMessage.Visibility = Visibility.Visible;

                    this.RestaurantsScroll.ViewChanged -= Scroll_ViewChanged;
                }
                else
                {
                    this.DefaultViewModel[RestaurantsPivotItemName] = FirstGroup;
                    if (FirstGroup.NextPageUri == null)
                    {
                        this.RestaurantsScroll.ViewChanged -= Scroll_ViewChanged;
                    }
                    else
                    {
                        this.RestaurantsScroll.ViewChanged += Scroll_ViewChanged;
                    }
                }                
            }       
        }

        private async void PromotedRestaurantsLoaded(object sender, RoutedEventArgs e)
        {
            if (this.PromotedRestaurantsMessage.Visibility == Visibility.Visible) this.PromotedRestaurantsMessage.Visibility = Visibility.Collapsed;
            if (this.RefreshButton.Visibility == Visibility.Visible) this.RefreshButton.Visibility = Visibility.Collapsed;

            this.PromotedRestaurantsProgressRing.IsActive = true;

            var SecondGroup = await MainPageSource.GetGroupAsync(PromotedRestaurantsPivotItemName);

            this.PromotedRestaurantsProgressRing.IsActive = false;

            if (SecondGroup == null)
            {
                this.PromotedRestaurantsMessage.Text = ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage");
                this.PromotedRestaurantsMessage.Visibility = Visibility.Visible;

                this.RefreshButton.Visibility = Visibility.Visible;

                this.MainPageCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                this.PromotedRestaurantsScroll.ViewChanged -= Scroll_ViewChanged;

                await ShowErrorAsync();
            }
            else
            {
                if (SecondGroup.Items.Count.Equals(0))
                {
                    this.PromotedRestaurantsMessage.Text = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyListErrorMessage");
                    this.PromotedRestaurantsMessage.Visibility = Visibility.Visible;

                    this.PromotedRestaurantsScroll.ViewChanged -= Scroll_ViewChanged;
                }  
                else
                {
                    this.DefaultViewModel[PromotedRestaurantsPivotItemName] = SecondGroup;
                    if (SecondGroup.NextPageUri == null)
                    {
                        this.PromotedRestaurantsScroll.ViewChanged -= Scroll_ViewChanged;
                    }
                    else
                    {
                        this.PromotedRestaurantsScroll.ViewChanged += Scroll_ViewChanged;
                    }
                }                
            }        
        }

        private async void CoursesLoaded(object sender, RoutedEventArgs e)
        {
            if (this.CoursesMessage.Visibility == Visibility.Visible) this.CoursesMessage.Visibility = Visibility.Collapsed;
            if (this.RefreshButton.Visibility == Visibility.Visible) this.RefreshButton.Visibility = Visibility.Collapsed;

            this.CoursesProgressRing.IsActive = true;
            this.CoursesToggle.IsEnabled = false;

            var ThirdGroup = await MainPageSource.GetGroupAsync(CoursesPivotItemName);

            this.CoursesProgressRing.IsActive = false;

            if (ThirdGroup == null)
            {
                this.CoursesMessage.Text = ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage");
                this.CoursesMessage.Visibility = Visibility.Visible;

                this.RefreshButton.Visibility = Visibility.Visible;
                
                this.CategoriesScroll.ViewChanged -= Scroll_ViewChanged;
                
                await ShowErrorAsync();
            }
            else
            {
                if (ThirdGroup.Items.Count.Equals(0))
                {
                    this.CoursesMessage.Text = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyListErrorMessage");
                    this.CoursesMessage.Visibility = Visibility.Visible;

                    this.CoursesScroll.ViewChanged -= Scroll_ViewChanged;
                }
                else
                {
                    this.DefaultViewModel[CoursesPivotItemName] = ThirdGroup;

                    if (ThirdGroup.NextPageUri == null)
                    {
                        this.CoursesScroll.ViewChanged -= Scroll_ViewChanged;
                    }
                    else
                    {
                        this.CoursesScroll.ViewChanged += Scroll_ViewChanged;
                    }
                }                
            }

            this.CoursesToggle.IsEnabled = true;
        }

        private async void CategoriesLoaded(object sender, RoutedEventArgs e)
        {
            if (this.CategoriesMessage.Visibility == Visibility.Visible) this.CategoriesMessage.Visibility = Visibility.Collapsed;
            if (this.RefreshButton.Visibility == Visibility.Visible) this.RefreshButton.Visibility = Visibility.Collapsed;

            this.CategoriesProgressRing.IsActive = true;

            var FifthGroup = await MainPageSource.GetGroupAsync(CategoriesPivotItemName);

            this.CategoriesProgressRing.IsActive = false;

            if (FifthGroup == null)
            {
                this.CategoriesMessage.Text = ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage");
                this.CategoriesMessage.Visibility = Visibility.Visible;

                this.RefreshButton.Visibility = Visibility.Visible;

                this.MainPageCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;

                this.CategoriesScroll.ViewChanged -= Scroll_ViewChanged;

                await ShowErrorAsync();
            }
            else
            {
                if (FifthGroup.Items.Count.Equals(0))
                {
                    this.CategoriesMessage.Text = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyListErrorMessage");
                    this.CategoriesMessage.Visibility = Visibility.Visible;

                    this.CategoriesScroll.ViewChanged -= Scroll_ViewChanged;
                }
                else
                {
                    this.DefaultViewModel[CategoriesPivotItemName] = FifthGroup;
                    if (FifthGroup.NextPageUri == null)
                    {
                        this.CategoriesScroll.ViewChanged -= Scroll_ViewChanged;
                    }
                    else
                    {
                        this.CategoriesScroll.ViewChanged += Scroll_ViewChanged;
                    }
                }                
            }
        }

        private async void CoursesToggle_Click(object sender, RoutedEventArgs e)
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("DownloadingMessage");
            statusBar.ProgressIndicator.ProgressValue = null;
            await statusBar.ProgressIndicator.ShowAsync();
            this.CoursesToggle.IsEnabled = false;

            var Label = (sender as AppBarButton).Label.ToString();

            if (Label == ResourceLoader.GetForCurrentView("Resources").GetString("CoursesToggleOne"))
            {
                var FourthGroup = await MainPageSource.GetGroupAsync(VegetariaCoursesPivotItemName);

                if (FourthGroup == null)
                {
                    await ShowErrorAsync();
                }
                else
                {
                    this.DefaultViewModel[CoursesPivotItemName] = FourthGroup;

                    if (FourthGroup.NextPageUri == null)
                    {
                        this.CoursesScroll.ViewChanged -= Scroll_ViewChanged;
                    }
                    else
                    {
                        this.CoursesScroll.ViewChanged += Scroll_ViewChanged;
                    }

                    if (this.CoursesMessage.Visibility == Visibility.Visible)
                    {
                        this.CoursesMessage.Visibility = Visibility.Collapsed;
                        this.RefreshButton.Visibility = Visibility.Collapsed;
                    }

                    this.CoursesToggle.Label = ResourceLoader.GetForCurrentView("Resources").GetString("CoursesToggleTwo");
                }
            }
            else if (Label == ResourceLoader.GetForCurrentView("Resources").GetString("CoursesToggleTwo"))
            {
                var ThirdGroup = await MainPageSource.GetGroupAsync(CoursesPivotItemName);

                if (ThirdGroup == null)
                {
                    await ShowErrorAsync();
                }
                else
                {
                    this.DefaultViewModel[CoursesPivotItemName] = ThirdGroup;

                    if (ThirdGroup.NextPageUri == null)
                    {
                        this.CoursesScroll.ViewChanged -= Scroll_ViewChanged;
                    }
                    else
                    {
                        this.CoursesScroll.ViewChanged += Scroll_ViewChanged;
                    }

                    if (this.CoursesMessage.Visibility == Visibility.Visible)
                    {
                        this.CoursesMessage.Visibility = Visibility.Collapsed;
                        this.RefreshButton.Visibility = Visibility.Collapsed;
                    }

                    this.CoursesToggle.Label = ResourceLoader.GetForCurrentView("Resources").GetString("CoursesToggleOne");
                }
            }

            await statusBar.ProgressIndicator.HideAsync();
            this.CoursesToggle.IsEnabled = true;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Сохраните здесь уникальное состояние страницы.
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

        #region Регистрация NavigationHelper

        /// <summary>
        /// Методы, предоставленные в этом разделе, используются исключительно для того, чтобы
        /// NavigationHelper для отклика на методы навигации страницы.
        /// <para>
        /// Логика страницы должна быть размещена в обработчиках событий для 
        /// <see cref="NavigationHelper.LoadState"/>
        /// и <see cref="NavigationHelper.SaveState"/>.
        /// Параметр навигации доступен в методе LoadState 
        /// в дополнение к состоянию страницы, сохраненному в ходе предыдущего сеанса.
        /// </para>
        /// </summary>
        /// <param name="e">Предоставляет данные для методов навигации и обработчики
        /// событий, которые не могут отменить запрос навигации.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void MainPagePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.MainPagePivot.SelectedIndex.Equals(RestaurantsPivotItemIndex) || this.MainPagePivot.SelectedIndex.Equals(PromotedRestaurantsPivotItemIndex))
            {
                this.FindButton.Visibility = Visibility.Visible;
                this.FavoriteButton.Visibility = Visibility.Visible;
                this.CoursesToggle.Visibility = Visibility.Collapsed;

                if (this.MainPagePivot.SelectedIndex.Equals(RestaurantsPivotItemIndex))
                {
                    if (this.RestarauntsMessage.Visibility == Visibility.Visible)
                    {
                        this.RefreshButton.Visibility = Visibility.Visible;
                    }
                }
                else if (this.MainPagePivot.SelectedIndex.Equals(PromotedRestaurantsPivotItemIndex))
                {
                    if (this.PromotedRestaurantsMessage.Visibility == Visibility.Visible)
                    {
                        this.RefreshButton.Visibility = Visibility.Visible;
                    }
                }
            }
            else if (this.MainPagePivot.SelectedIndex.Equals(CategoriesPivotItemIndex))
            {
                this.FindButton.Visibility = Visibility.Visible;
                this.FavoriteButton.Visibility = Visibility.Collapsed;
                this.CoursesToggle.Visibility = Visibility.Collapsed;

                if (this.CategoriesMessage.Visibility == Visibility.Visible)
                {
                    this.RefreshButton.Visibility = Visibility.Visible;
                }
            }
            else if (this.MainPagePivot.SelectedIndex.Equals(CoursesPivotItemIndex))
            {
                this.FindButton.Visibility = Visibility.Visible;
                this.FavoriteButton.Visibility = Visibility.Visible;
                this.CoursesToggle.Visibility = Visibility.Visible;

                if (this.CoursesMessage.Visibility == Visibility.Visible)
                {
                    this.RefreshButton.Visibility = Visibility.Visible;
                }
            }
        }

        private async void Scroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                var Sender = sender as ScrollViewer;
                if (Sender.ScrollableHeight - Sender.VerticalOffset == 0)
                {
                    lock(IncrementalLoadingHelper)
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
                                if (GroupName == RestaurantsPivotItemName)
                                {
                                    this.RestaurantsScroll.ViewChanged -= Scroll_ViewChanged;
                                }
                                else if (GroupName == PromotedRestaurantsPivotItemName)
                                {
                                    this.PromotedRestaurantsScroll.ViewChanged -= Scroll_ViewChanged;
                                }
                                else if (GroupName == CoursesPivotItemName || GroupName == VegetariaCoursesPivotItemName)
                                {
                                    this.CoursesScroll.ViewChanged -= Scroll_ViewChanged;
                                }
                                else if (GroupName == CategoriesPivotItemName)
                                {
                                    this.CategoriesScroll.ViewChanged -= Scroll_ViewChanged;
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
                        if (GroupName == RestaurantsPivotItemName)
                        {
                            this.RestaurantsScroll.ViewChanged -= Scroll_ViewChanged;
                        }
                        else if (GroupName == PromotedRestaurantsPivotItemName)
                        {
                            this.PromotedRestaurantsScroll.ViewChanged -= Scroll_ViewChanged;
                        }
                        else if (GroupName == CoursesPivotItemName || GroupName == VegetariaCoursesPivotItemName)
                        {
                            this.CoursesScroll.ViewChanged -= Scroll_ViewChanged;
                        }
                        else if (GroupName == CategoriesPivotItemName)
                        {
                            this.CategoriesScroll.ViewChanged -= Scroll_ViewChanged;
                        }
                    }

                    lock (IncrementalLoadingHelper)
                    {
                        isIncrementalLoadingStarted = false;
                    }
                }
            }
        }

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

        private void CategoryClick(object sender, ItemClickEventArgs e)
        {
            int RestaurantId = -1;
            int CategoryId = ((Item)e.ClickedItem).Id;
            string NavigationPagement = RestaurantId.ToString() + ":" + CategoryId.ToString();

            if (!Frame.Navigate(typeof(CategoryPage), NavigationPagement))
            {
                throw new Exception();
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.RefreshButton.IsEnabled = false;
            string GroupName = RestaurantsPivotItemName;
            if (this.MainPagePivot.SelectedIndex.Equals(PromotedRestaurantsPivotItemIndex))
            {
                GroupName = PromotedRestaurantsPivotItemName;
            }
            else if (this.MainPagePivot.SelectedIndex.Equals(CoursesPivotItemIndex))
            {
                GroupName = CoursesPivotItemName;
            }
            else if (this.MainPagePivot.SelectedIndex.Equals(CategoriesPivotItemIndex))
            {
                GroupName = CategoriesPivotItemName;
            }

            if (!this.DefaultViewModel.ContainsKey(GroupName))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("DownloadingMessage");
                statusBar.ProgressIndicator.ProgressValue = null;
                await statusBar.ProgressIndicator.ShowAsync();

                var Group = await MainPageSource.GetGroupAsync(GroupName);

                await statusBar.ProgressIndicator.HideAsync();

                if (Group == null)
                {
                    this.RefreshButton.IsEnabled = true;

                    this.CoursesMessage.Visibility = Visibility.Visible;
                    this.CoursesMessage.Text = ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage");

                    this.RefreshButton.Visibility = Visibility.Visible;
                    await ShowErrorAsync();
                }
                else
                {
                    this.RefreshButton.IsEnabled = true;
                    this.RefreshButton.Visibility = Visibility.Collapsed;
                    if (GroupName == RestaurantsPivotItemName)
                    {
                        this.RestarauntsMessage.Visibility = Visibility.Collapsed;
                    }
                    else if (GroupName == PromotedRestaurantsPivotItemName)
                    {
                        this.PromotedRestaurantsMessage.Visibility = Visibility.Collapsed;
                    }
                    else if (GroupName == CoursesPivotItemName)
                    {
                        this.CoursesMessage.Visibility = Visibility.Collapsed;
                    }
                    else if (GroupName == CategoriesPivotItemName)
                    {
                        this.CategoriesMessage.Visibility = Visibility.Collapsed;
                    }
                    this.DefaultViewModel[GroupName] = Group;
                }
            }
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            string NavigationParametr = RestaurantsPivotItemName;
            
            if (this.MainPagePivot.SelectedIndex.Equals(CoursesPivotItemIndex))
            {
                NavigationParametr = CoursesPivotItemName;
            }
            else if (this.MainPagePivot.SelectedIndex.Equals(CategoriesPivotItemIndex))
            {
                NavigationParametr = CategoriesPivotItemName;
            }

            if (!Frame.Navigate(typeof(SearchPage), NavigationParametr))
            {
                throw new Exception();
            }
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            string NavigationParametr = RestaurantsPivotItemName;

            if (this.MainPagePivot.SelectedIndex.Equals(CoursesPivotItemIndex))
            {
                NavigationParametr = CoursesPivotItemName;
            }

            if (!Frame.Navigate(typeof(FavoritesPage), NavigationParametr))
            {
                throw new Exception();
            }
        }

        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(SettingsPage), null))
            {
                throw new Exception();
            }
        }
    }
}
