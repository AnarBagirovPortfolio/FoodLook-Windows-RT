using FoodLook_2.Common;
using FoodLook_2.DataModel;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace FoodLook_2
{
    public sealed partial class SearchPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        private const string ByName = "Name";
        private const string ByPrice = "Price";
        private const string ByPlace = "Place";

        private static string SearchParametrTwo = ByName;
        private static string SearchParametrOne = MainPage.RestaurantsPivotItemName;

        public static string StatusBarDisplayedName = null;

        private object IncrementalLoadingHelper = new object();
        private bool isIncrementalLoadingWorking = false;

        public SearchPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        async void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
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
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.ProgressValue = 0;

            this.BaseGrid.Width = Window.Current.Bounds.Width - 2 * 19 + 4;

            if (e.NavigationParameter.ToString().Equals(MainPage.RestaurantsPivotItemName))
            {
                statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("SearchRestaurants");
                StatusBarDisplayedName = ResourceLoader.GetForCurrentView("Resources").GetString("SearchRestaurants");
                SearchParametrOne = MainPage.RestaurantsPivotItemName;
            }
            else if (e.NavigationParameter.ToString().Equals(MainPage.CoursesPivotItemName))
            {
                statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("SearchCourses");
                StatusBarDisplayedName = ResourceLoader.GetForCurrentView("Resources").GetString("SearchCourses");
                SearchParametrOne = MainPage.CoursesPivotItemName;

                this.ByPlaceButton.Visibility = Visibility.Collapsed;
            }
            else if (e.NavigationParameter.ToString().Equals(MainPage.CategoriesPivotItemName))
            {
                statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("SearchCategories");
                StatusBarDisplayedName = ResourceLoader.GetForCurrentView("Resources").GetString("SearchCategories");
                SearchParametrOne = MainPage.CategoriesPivotItemName;

                this.ByNameButton.Visibility = Visibility.Collapsed;
                this.ByPriceButton.Visibility = Visibility.Collapsed;
                this.ByPlaceButton.Visibility = Visibility.Collapsed;
            }

            if (this.SearchByNameTextBox.Visibility == Visibility.Visible)
            {
                SearchParametrTwo = ByName;
            }
            else if (this.LeftPrice.Visibility == Visibility.Visible)
            {
                SearchParametrTwo = ByPrice;
            }
            else if (this.SearchByPlaceTextBox.Visibility == Visibility.Visible)
            {
                SearchParametrTwo = ByPlace;
            }

            if (this.ByPlaceButton.Visibility == Visibility.Visible)
            {
                if (new Geolocator().LocationStatus == PositionStatus.Disabled)
                {
                    this.ByPlaceButton.IsEnabled = false;
                }
            }

            await statusBar.ProgressIndicator.ShowAsync();
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

        private void ByNameButton_Click(object sender, RoutedEventArgs e)
        {
            if (SerchByPriceGrid.Visibility == Visibility.Visible)
            {
                this.ByPriceButton.Visibility = Visibility.Visible;
                this.SerchByPriceGrid.Visibility = Visibility.Collapsed;
            }

            if (this.SearchByPlaceTextBox.Visibility == Visibility.Visible)
            {
                this.ByPlaceButton.Visibility = Visibility.Visible;
                this.SearchByPlaceTextBox.Visibility = Visibility.Collapsed;
            }

            this.LeftPrice.Text = String.Empty;
            this.RightPrice.Text = String.Empty;
            this.SearchByPlaceTextBox.Text = String.Empty;

            this.DefaultViewModel["item"] = new SearchAnswer();

            this.ByNameButton.Visibility = Visibility.Collapsed;
            this.SearchByNameTextBox.Visibility = Visibility.Visible;

            this.AnswerListView.ItemTemplate = this.Resources["DataTemplate1"] as DataTemplate;

            if (this.SearchErrorTextBlock.Visibility == Visibility.Visible)
            {
                this.SearchErrorTextBlock.Visibility = Visibility.Collapsed;
            }

            SearchParametrTwo = ByName;
        }

        private void ByPriceButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SearchByNameTextBox.Visibility == Visibility.Visible)
            {
                this.ByNameButton.Visibility = Visibility.Visible;
                this.SearchByNameTextBox.Visibility = Visibility.Collapsed;
            }

            if (this.SearchByPlaceTextBox.Visibility == Visibility.Visible)
            {
                this.ByPlaceButton.Visibility = Visibility.Visible;
                this.SearchByPlaceTextBox.Visibility = Visibility.Collapsed;
            }

            this.SearchByNameTextBox.Text = String.Empty;
            this.SearchByPlaceTextBox.Text = String.Empty;

            this.DefaultViewModel["item"] = new SearchAnswer();

            this.ByPriceButton.Visibility = Visibility.Collapsed;
            this.SerchByPriceGrid.Visibility = Visibility.Visible;

            this.AnswerListView.ItemTemplate = this.Resources["DataTemplate1"] as DataTemplate;

            if (this.SearchErrorTextBlock.Visibility == Visibility.Visible)
            {
                this.SearchErrorTextBlock.Visibility = Visibility.Collapsed;
            }

            SearchParametrTwo = ByPrice;
        }

        private void ByPlaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SearchByNameTextBox.Visibility == Visibility.Visible)
            {
                this.ByNameButton.Visibility = Visibility.Visible;
                this.SearchByNameTextBox.Visibility = Visibility.Collapsed;
            }

            if (SerchByPriceGrid.Visibility == Visibility.Visible)
            {
                this.ByPriceButton.Visibility = Visibility.Visible;
                this.SerchByPriceGrid.Visibility = Visibility.Collapsed;
            }

            this.LeftPrice.Text = String.Empty;
            this.RightPrice.Text = String.Empty;
            this.SearchByNameTextBox.Text = String.Empty;

            this.DefaultViewModel["item"] = new SearchAnswer();

            this.ByPlaceButton.Visibility = Visibility.Collapsed;
            this.SearchByPlaceTextBox.Visibility = Visibility.Visible;

            this.AnswerListView.ItemTemplate = this.Resources["DataTemplate2"] as DataTemplate;

            if (this.SearchErrorTextBlock.Visibility == Visibility.Visible)
            {
                this.SearchErrorTextBlock.Visibility = Visibility.Collapsed;
            }

            SearchParametrTwo = ByPlace;
        }

        private async void ShowMessageAsync(string message)
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = message;
            statusBar.ProgressIndicator.ProgressValue = 0;
            await statusBar.ProgressIndicator.ShowAsync();

            await Task.Delay(1000);

            if (StatusBarDisplayedName.Equals(null))
            {
                await statusBar.ProgressIndicator.HideAsync();
            }
            else
            {
                statusBar.ProgressIndicator.Text = StatusBarDisplayedName;
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            this.SearchButton.IsEnabled = false;
            this.SearchPageCommandBar.IsEnabled = false;
            this.SearchErrorTextBlock.Visibility = Visibility.Collapsed;

            this.DefaultViewModel["item"] = new SearchAnswer();

            string RequestUri = null;

            if (SearchParametrOne == MainPage.RestaurantsPivotItemName)
            {
                RequestUri = "https://www.foodlook.az/api/restaurants/search/";
            }
            else if (SearchParametrOne == MainPage.CoursesPivotItemName)
            {
                RequestUri = "https://www.foodlook.az/api/courses/search/";
            }
            else if (SearchParametrOne == MainPage.CategoriesPivotItemName)
            {
                RequestUri = "https://www.foodlook.az/api/categories/search/";
            }

            if (SearchParametrTwo == ByName)
            {
                if (this.SearchByNameTextBox.Text == String.Empty)
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("EmptyErrorMessage"));
                }
                else
                {
                    RequestUri += "?q=" + this.SearchByNameTextBox.Text;

                    this.progressRing.IsActive = true;

                    var Result = await SearchSource.GetSearchAnswer(new Uri(RequestUri));

                    this.progressRing.IsActive = false;

                    if (Result == null)
                    {
                        this.SearchErrorTextBlock.Text = ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage");
                        this.SearchErrorTextBlock.Visibility = Visibility.Visible;
                    }
                    else if (Result.SearchResults.Count.Equals(0))
                    {
                        this.SearchErrorTextBlock.Text = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyResultErrorMessage");
                        this.SearchErrorTextBlock.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.DefaultViewModel["item"] = Result;
                    }
                }
            }
            else if (SearchParametrTwo == ByPrice)
            {
                if (this.LeftPrice.Text.Equals(String.Empty) || this.RightPrice.Text.Equals(String.Empty))
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("EmptyErrorMessage"));
                }
                else
                {
                    RequestUri += "price/from/" + this.LeftPrice.Text.ToString().Replace(',', '.') + "/to/" + this.RightPrice.Text.ToString().Replace(',', '.') + "/";

                    this.progressRing.IsActive = true;

                    var Result = await SearchSource.GetSearchAnswer(new Uri(RequestUri));

                    this.progressRing.IsActive = false;

                    if (Result == null)
                    {
                        this.SearchErrorTextBlock.Text = ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage");
                        this.SearchErrorTextBlock.Visibility = Visibility.Visible;
                    }
                    else if (Result.SearchResults.Count.Equals(0))
                    {
                        this.SearchErrorTextBlock.Text = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyResultErrorMessage");
                        this.SearchErrorTextBlock.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.DefaultViewModel["item"] = Result;
                    }
                }
            }
            else if (SearchParametrTwo == ByPlace)
            {
                if (this.SearchByPlaceTextBox.Text.Equals(string.Empty))
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("EmptyErrorMessage"));
                }
                else
                {
                    Geolocator geoLocator = new Geolocator();

                    if (geoLocator.LocationStatus.Equals(PositionStatus.Disabled))
                    {
                        ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("LocationErrorMessage"));
                    }
                    else
                    {
                        this.progressRing.IsActive = true;

                        Geoposition geoPosition = await geoLocator.GetGeopositionAsync();
                        string Latitude = Math.Round(geoPosition.Coordinate.Point.Position.Latitude, 5).ToString(CultureInfo.InvariantCulture);
                        string Longitude = Math.Round(geoPosition.Coordinate.Point.Position.Longitude, 6).ToString(CultureInfo.InvariantCulture);
                        string Radius = this.SearchByPlaceTextBox.Text.Replace(',', '.');

                        RequestUri = "https://www.foodlook.az/api/restaurants/search/location/latitude/" + Latitude + "/longitude/" + Longitude + "/radius/" + Radius + "/";

                        //RequestUri = "https://www.foodlook.az/api/restaurants/search/location/latitude/40.39236/longitude/49.821542/radius/5/";

                        var Result = await SearchSource.GetSearchAnswer(new Uri(RequestUri));

                        this.progressRing.IsActive = false;

                        if (Result == null)
                        {
                            this.SearchErrorTextBlock.Text = ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage");
                            this.SearchErrorTextBlock.Visibility = Visibility.Visible;
                        }
                        else if (Result.SearchResults.Count.Equals(0))
                        {
                            this.SearchErrorTextBlock.Text = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyResultErrorMessage");
                            this.SearchErrorTextBlock.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.DefaultViewModel["item"] = Result;
                        }
                    }                    
                }
            }
            else
            {
                ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("SomeError"));
            }

            this.SearchButton.IsEnabled = true;
            this.SearchPageCommandBar.IsEnabled = true;
        }

        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                var Sender = sender as ScrollViewer;
                if (Sender.ScrollableHeight - Sender.VerticalOffset == 0)
                {
                    var CurrentAnswer = this.DefaultViewModel["item"] as SearchAnswer;

                    if (CurrentAnswer.Next == null)
                    {
                        this.Scroll.ViewChanged -= ScrollViewer_ViewChanged;
                    }
                    else
                    {
                        lock (IncrementalLoadingHelper)
                        {
                            if (isIncrementalLoadingWorking)
                            {
                                return;
                            }
                            else
                            {
                                isIncrementalLoadingWorking = true;
                            }
                        }

                        var statusBar = StatusBar.GetForCurrentView();
                        statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("DownloadingMessage");
                        statusBar.ProgressIndicator.ProgressValue = null;
                        await statusBar.ProgressIndicator.ShowAsync();

                        bool isSuccess = await SearchSource.LoadMoreItemsAsync(CurrentAnswer.Next);

                        await statusBar.ProgressIndicator.HideAsync();

                        if (isSuccess)
                        {
                            if ((this.DefaultViewModel["item"] as SearchAnswer).Next == null)
                            {
                                this.Scroll.ViewChanged -= ScrollViewer_ViewChanged;
                            }
                        }
                        else
                        {
                            ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
                        }

                        lock (IncrementalLoadingHelper)
                        {
                            isIncrementalLoadingWorking = false;
                        }
                    }
                }
            }
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var ClickedItem = (SearchResult)e.ClickedItem;

            if (SearchParametrOne == MainPage.RestaurantsPivotItemName)
            {
                if (this.ByPlaceButton.Visibility == Visibility.Collapsed)
                {
                    var CurrentLocation = ClickedItem.ItemLocation;

                    string Latitude = (CurrentLocation.Latitude == -1) ? null : CurrentLocation.Latitude.ToString().Replace(',', '.');
                    string Longitude = (CurrentLocation.Longitude == -1) ? null : CurrentLocation.Longitude.ToString().Replace(',', '.');
                    string Name = ClickedItem.Label;

                    if (Latitude != null && Longitude != null)
                    {
                        Name.Replace(" ", "%20");
                        string RequestUriString = "bingmaps:?collection=point." + Latitude + "_" + Longitude + "_" + Name + "&lvl=16";
                        Uri RequestUri = new Uri(RequestUriString);

                        bool Successful = await Launcher.LaunchUriAsync(RequestUri);

                        if (!Successful)
                        {
                            ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("MapError"));
                        }
                    }
                    else
                    {
                        ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("MapError"));
                    }
                }
                else
                {
                    if (!Frame.Navigate(typeof(RestaurantPage), ClickedItem.Id))
                    {
                        throw new Exception();
                    }
                }                
            }
            else if (SearchParametrOne == MainPage.CoursesPivotItemName)
            {
                if (!Frame.Navigate(typeof(CoursePage), ClickedItem.Id))
                {
                    throw new Exception();
                }
            }
            else if (SearchParametrOne == MainPage.CategoriesPivotItemName)
            {
                int RestaurantId = -1;
                int CategoryId = ClickedItem.Id;
                string NavigationPagement = RestaurantId.ToString() + ":" + CategoryId.ToString();

                if (!Frame.Navigate(typeof(CategoryPage), NavigationPagement))
                {
                    throw new Exception();
                }
            }

            await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
        }

        private void RestaurantLocation_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;

            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private async void OpenMaps_Click(object sender, RoutedEventArgs e)
        {
            var senderElement = sender as MenuFlyoutItem;
            string ElementTag = senderElement.Tag.ToString();

            if (ElementTag == null)
            {
                ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
            }
            else
            {
                int ItemId = Convert.ToInt32(ElementTag);

                if (!Frame.Navigate(typeof(RestaurantPage), ItemId))
                {
                    throw new Exception();
                }

                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }            
        }
    }
}
