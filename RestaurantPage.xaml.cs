using FoodLook_2.Common;
using FoodLook_2.Data;
using FoodLook_2.DataModel;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FoodLook_2
{
    public sealed partial class RestaurantPage : Page
    {
        public const string Label = "Label";
        public const string Restaurant = "Restaurant";
        public const string Comments = "Comments";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        private bool isIncrementalLoadingStarted = false;
        private Object IncrementalLoadingHelper = new object();

        public RestaurantPage()
        {
            this.InitializeComponent();

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
            int Id = (int)e.NavigationParameter;
            var Item = await RestaurantPageSource.GetRestaurantAsync(Id);

            this.ProgressRing.IsActive = false;

            if (Item == null)
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();

                    await ShowErrorAsync();
                }
                else
                {
                    if (!Frame.Navigate(typeof(MainPage), "FoodLook 2"))
                    {
                        throw new Exception();
                    }
                    Frame.BackStack.Clear();

                    await ShowErrorAsync();
                }
            }
            else
            {
                this.DefaultViewModel[Restaurant] = Item;
                this.DefaultViewModel[Label] = Item.Label;

                LikeButtonToggle();
                FavoriteButtonToggle();

                this.RestaurantPageCommandBar.Visibility = Visibility.Visible;

                if (Item.Website == null)
                {
                    this.WebsiteBlock.Visibility = Visibility.Collapsed;
                }
                if (Item.Email == null)
                {
                    this.EmailBlock.Visibility = Visibility.Collapsed;
                }
                if (Item.Facebook == null)
                {
                    this.FacebookBlock.Visibility = Visibility.Collapsed;
                }
                if (Item.Instagram == null)
                {
                    this.InstagramBlock.Visibility = Visibility.Collapsed;
                }
                if (!Item.Parking)
                {
                    this.ParkingImage.Opacity = 0.4;
                }
                if (!Item.PaymentCards)
                {
                    this.PaymentCardsImage.Opacity = 0.4;
                }
                if (!Item.LiveMusic)
                {
                    this.LiveMusicImage.Opacity = 0.4;
                }
                if (Item.Telephone == null)
                {
                    this.PhoneButton.Visibility = Visibility.Collapsed;
                }

                this.RestaurantPagePivot.Visibility = Visibility.Visible;
            }
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void WebUri_Click(object sender, RoutedEventArgs e)
        {
            var SenderButton = sender as Button;
            string ButtonTag = SenderButton.Tag.ToString();

            if (ButtonTag != null)
            {
                Uri RequestUri = new Uri(ButtonTag);

                await Launcher.LaunchUriAsync(RequestUri);
            }            
            else
            {
                await ShowErrorAsync();
            }
        }

        private async void Email_Click(object sender, RoutedEventArgs e)
        {
            var SenderButton = sender as Button;
            string ButtonTag = SenderButton.Tag.ToString();

            if (ButtonTag != null)
            {
                EmailMessage Message = new EmailMessage();
                EmailRecipient SendTo = new EmailRecipient();
                SendTo.Address = ButtonTag;
                Message.To.Add(SendTo);
                await EmailManager.ShowComposeNewEmailAsync(Message);
            }
            else
            {
                await ShowErrorAsync();
            }
        }

        private async void Location_Click(object sender, ItemClickEventArgs e)
        {
            var ClickedItem = (Location)e.ClickedItem;

            string Latitude = (ClickedItem.Latitude == -1) ? null : ClickedItem.Latitude.ToString(CultureInfo.InvariantCulture);
            string Longitude = (ClickedItem.Longitude == -1) ? null : ClickedItem.Longitude.ToString(CultureInfo.InvariantCulture);
            string Name = this.DefaultViewModel[Label] as string;

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

        private async void PhoneButton_Click(object sender, RoutedEventArgs e)
        {
            var Item = this.DefaultViewModel[Restaurant] as Restaurant;

            string Telephone = Item.Telephone;

            if (Telephone != null)
            {
                Telephone = Regex.Replace(Telephone, "[^0-9]", "");

                if (Telephone.Length == 10)
                {
                    PhoneCallManager.ShowPhoneCallUI(Telephone, Item.Label);
                }
                else if (Telephone.Length == 12)
                {
                    Telephone = "+" + Telephone;
                    PhoneCallManager.ShowPhoneCallUI(Telephone, Item.Label);
                }
                else
                {
                    await ShowErrorAsync();
                }
            }
            else
            {
                this.PhoneButton.IsEnabled = false;
            }
        }

        private void RestaurantPagePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.RestaurantPagePivot.SelectedItem == this.Information)
            {
                RestaurantPageCommandBar.Visibility = Visibility.Visible;
            }
            else if (this.RestaurantPagePivot.SelectedItem == this.Menu || this.RestaurantPagePivot.SelectedItem == this.Photos || this.RestaurantPagePivot.SelectedItem == this.RestaurantsComments)
            {
                RestaurantPageCommandBar.Visibility = Visibility.Collapsed;
            }
        }

        private void LikeButtonToggle()
        {
            var Item = this.DefaultViewModel[Restaurant] as Restaurant;

            if (Item.RestaurantSocial.LikeId == -1)
            {
                this.LikeButton.Icon = new SymbolIcon(Symbol.Like);
                this.LikeButton.Label = ResourceLoader.GetForCurrentView("Resources").GetString("Like");
            }
            else
            {
                this.LikeButton.Icon = new SymbolIcon(Symbol.Dislike);
                this.LikeButton.Label = ResourceLoader.GetForCurrentView("Resources").GetString("UnLike");
            }
        }

        private void FavoriteButtonToggle()
        {
            var Item = this.DefaultViewModel[Restaurant] as Restaurant;

            if (Item.RestaurantSocial.FavoriteId == -1)
            {
                this.FavoriteButton.Icon = new SymbolIcon(Symbol.Favorite);
                this.FavoriteButton.Label = ResourceLoader.GetForCurrentView("Resources").GetString("Favorite");
            }
            else
            {
                this.FavoriteButton.Icon = new SymbolIcon(Symbol.UnFavorite);
                this.FavoriteButton.Label = ResourceLoader.GetForCurrentView("Resources").GetString("UnFavorite");
            }
        }

        private async void ShowMessageAsync(string message)
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = message;
            statusBar.ProgressIndicator.ProgressValue = 0;
            await statusBar.ProgressIndicator.ShowAsync();

            await Task.Delay(1000);

            await statusBar.ProgressIndicator.HideAsync();
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            var Item = this.DefaultViewModel[Restaurant] as Restaurant;

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("UpdateMessage");
            statusBar.ProgressIndicator.ProgressValue = null;
            await statusBar.ProgressIndicator.ShowAsync();

            this.LikeButton.IsEnabled = false;

            if (Item.RestaurantSocial.LikeId == -1)
            {
                bool isSuccess = await RestaurantPageSource.AddLikeAsync(Item.Id);

                await statusBar.ProgressIndicator.HideAsync();

                if (isSuccess)
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("LikeSuccessfulAdding"));
                }
                else
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
                }

                LikeButtonToggle();
            }
            else
            {
                bool isSuccess = await RestaurantPageSource.RemoveLikeAsync(Item.Id);

                await statusBar.ProgressIndicator.HideAsync();

                if (isSuccess)
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("LikeSuccessfulRemoving"));
                }
                else
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
                }

                LikeButtonToggle();
            }

            this.LikeButton.IsEnabled = true;
        }

        private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            var Item = this.DefaultViewModel[Restaurant] as Restaurant;

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("UpdateMessage");
            statusBar.ProgressIndicator.ProgressValue = null;
            await statusBar.ProgressIndicator.ShowAsync();

            this.FavoriteButton.IsEnabled = false;

            if (Item.RestaurantSocial.FavoriteId == -1)
            {
                bool isSuccess = await RestaurantPageSource.AddToFavoriteAsync(Item.Id);

                await statusBar.ProgressIndicator.HideAsync();

                if (isSuccess)
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("FavoriteSuccessfulAdding"));
                }
                else
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
                }

                FavoriteButtonToggle();
            }
            else
            {
                bool isSuccess = await RestaurantPageSource.RemoveFromFavoriteAsync(Item.Id);

                await statusBar.ProgressIndicator.HideAsync();

                if (isSuccess)
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("FavoriteSuccessfulRemoving"));
                }
                else
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
                }

                FavoriteButtonToggle();
            }

            this.FavoriteButton.IsEnabled = true;
        }

        private void CommentInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.NoCommentsTextBlock.Visibility = Visibility.Collapsed;
        }

        private void CommentInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((this.DefaultViewModel[Comments] as CommentsGroup).Comments.Count() == 0)
            {
                this.NoCommentsTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                this.NoCommentsTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private async void CommentsList_Loaded(object sender, RoutedEventArgs e)
        {
            this.CommentsLoadedProgressRing.IsActive = true;

            var Item = this.DefaultViewModel[Restaurant] as Restaurant;

            var CommentsList = await CommentsSource.GetCommentsAsync(Item.Id);

            this.CommentsLoadedProgressRing.IsActive = false;

            if (CommentsList == null)
            {
                await ShowErrorAsync();
            }
            else
            {
                this.DefaultViewModel[Comments] = CommentsList;

                if (CommentsList.Comments.Count() == 0)
                {
                    this.NoCommentsTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    this.NoCommentsTextBlock.Visibility = Visibility.Collapsed;
                }

                this.CommentInputTextBox.IsEnabled = true;

                if (CommentsList.NextPageUri == null)
                {
                    this.CommentsScrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
                }
            }
        }

        private void Comment_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;

            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            var SenderItem = senderElement.DataContext as CommentItem;

            if (SenderItem.User.Name == Api.Settings.Values[Api.Username].ToString())
            {
                flyoutBase.ShowAt(senderElement);
            }
        }

        private async void CommentInputTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                this.CommentInputTextBox.IsEnabled = false;

                var Text = this.CommentInputTextBox.Text.ToString();

                if (Text.Length == 0)
                {
                    await ShowErrorAsync();
                }
                else
                {
                    var Item = this.DefaultViewModel[Restaurant] as Restaurant;

                    var statusBar = StatusBar.GetForCurrentView();
                    statusBar.ProgressIndicator.ProgressValue = null;
                    statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("AddingMessage");
                    await statusBar.ProgressIndicator.ShowAsync();

                    bool isSuccess = await CommentsSource.AddCommentAsync(Item.Id, Text);

                    await statusBar.ProgressIndicator.HideAsync();

                    if (isSuccess)
                    {
                        this.CommentInputTextBox.Text = "";
                        this.CommentsScrollViewer.ChangeView(0, 0, this.CommentsScrollViewer.ZoomFactor);
                        ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("CommentSuccessfulAdding"));

                        if (this.NoCommentsTextBlock.Visibility == Visibility.Visible)
                        {
                            this.NoCommentsTextBlock.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
                    }

                    this.CommentInputTextBox.IsEnabled = true;
                }
            }
        }

        private async void RemoveCommentClick(object sender, RoutedEventArgs e)
        {          
            var senderElement = sender as MenuFlyoutItem;
            var CommentIdString = senderElement.Tag.ToString();

            if (CommentIdString.Length == 0)
            {
                ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
            }
            else
            {
                var Item = this.DefaultViewModel[Restaurant] as Restaurant;

                int CommentId = Convert.ToInt32(CommentIdString);

                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.ProgressValue = null;
                statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("RemoveMessage");
                await statusBar.ProgressIndicator.ShowAsync();

                bool isSuccess = await CommentsSource.RemoveCommentAsync(Item.Id, CommentId);

                await statusBar.ProgressIndicator.HideAsync();

                if (isSuccess)
                {
                    ResourceLoader.GetForCurrentView("Resources").GetString("CommentSuccessfulRemoving");

                    if ((this.DefaultViewModel[Comments] as CommentsGroup).Comments.Count() == 0)
                    {
                        this.NoCommentsTextBlock.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.NoCommentsTextBlock.Visibility = Visibility.Collapsed;
                    }  
                }
                else
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
                }
            }
        }

        private async void ChangeCommentClick(object sender, RoutedEventArgs e)
        {
            var senderElement = sender as MenuFlyoutItem;

            ContentDialog ChangeCommentDialog = new ContentDialog();
            StatusBar.GetForCurrentView().ForegroundColor = Colors.Black;
            ChangeCommentDialog.Closed += ChangeCommentDialog_Closed;
            ChangeCommentDialog.PrimaryButtonClick += ChangeCommentDialog_PrimaryButtonClick;

            ChangeCommentDialog.Title = ResourceLoader.GetForCurrentView("Resources").GetString("NewCommentTitle");

            TextBox InputBox = new TextBox();
            InputBox.Tag = senderElement.Tag;
            InputBox.SelectionHighlightColor = new SolidColorBrush(Color.FromArgb(255, 204, 0, 0));
            InputBox.Background = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));
            InputBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));

            ChangeCommentDialog.Content = InputBox;
            ChangeCommentDialog.PrimaryButtonText = ResourceLoader.GetForCurrentView("Resources").GetString("ChangeLabel");
            ChangeCommentDialog.SecondaryButtonText = ResourceLoader.GetForCurrentView("Resources").GetString("CancelLabel");
            ChangeCommentDialog.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
            ChangeCommentDialog.Foreground = new SolidColorBrush(Colors.Black);
            ChangeCommentDialog.RequestedTheme = ElementTheme.Light;

            await ChangeCommentDialog.ShowAsync();
        }

        async void ChangeCommentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var InputBox = sender.Content as TextBox;
            if (InputBox.Text.Length == 0)
            {
                ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("EmptyCommentError"));
            }
            else
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.ProgressValue = null;
                statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("UpdateMessage");
                await statusBar.ProgressIndicator.ShowAsync();

                var Item = this.DefaultViewModel[Restaurant] as Restaurant;

                var Entity = Item.Id;
                var Id = Convert.ToInt32(InputBox.Tag.ToString());
                var NewText = InputBox.Text;

                bool isSuccess = await CommentsSource.ChangeCommentAsync(Entity, Id, NewText);

                await statusBar.ProgressIndicator.HideAsync();

                if (isSuccess)
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("CommentSuccessfulChanging"));
                }
                else
                {
                    ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
                }
            }
        }

        void ChangeCommentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
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

                    var CommentsGroup = this.DefaultViewModel[Comments] as CommentsGroup;

                    if (CommentsGroup.NextPageUri == null)
                    {
                        this.CommentsScrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
                    }
                    else
                    {
                        var statusBar = StatusBar.GetForCurrentView();
                        statusBar.ProgressIndicator.Text = ResourceLoader.GetForCurrentView("Resources").GetString("DownloadingMessage");
                        statusBar.ProgressIndicator.ProgressValue = null;
                        await statusBar.ProgressIndicator.ShowAsync();

                        bool isSuccess = await CommentsSource.LoadMoreCommentsAsync(CommentsGroup.Id);

                        await statusBar.ProgressIndicator.HideAsync();

                        if (isSuccess)
                        {
                            var UpdatedGroup = this.DefaultViewModel[Comments] as CommentsGroup;
                            if (UpdatedGroup.NextPageUri == null)
                            {
                                this.CommentsScrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
                            }
                        }
                        else
                        {
                            await ShowErrorAsync();
                        }
                    }

                    lock (IncrementalLoadingHelper)
                    {
                        isIncrementalLoadingStarted = false;
                    }
                }
            }
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            //Картинка загрузилась
        }

        private void Category_ItemClick(object sender, ItemClickEventArgs e)
        {
            var Item = this.DefaultViewModel[Restaurant] as Restaurant;

            int RestaurantId = Item.Id;
            int CategoryId = ((MenuItem)e.ClickedItem).Id;

            string NavigationPagement = RestaurantId.ToString() + ":" + CategoryId.ToString();

            if (!Frame.Navigate(typeof(CategoryPage), NavigationPagement))
            {
                throw new Exception();
            }
        }
    }
}
