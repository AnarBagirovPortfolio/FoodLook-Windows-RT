using FoodLook_2.Common;
using FoodLook_2.Data;
using FoodLook_2.DataModel;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FoodLook_2
{
    public sealed partial class CategoryPage : Page
    {
        public const string Item = "Item";
        public const string Label = "Label";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        private bool isIncrementalLoadingWorking = false;
        private Object IncrementalLoadingHelper = new object();

        public CategoryPage()
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
            this.progressRing.IsActive = true;

            Category DisplayedCategory = await CategoriesSource.GetCategoryAsync((string)e.NavigationParameter);

            this.progressRing.IsActive = false;

            if (DisplayedCategory == null)
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                else
                {
                    if (!Frame.Navigate(typeof(MainPage), "FoodLook 2"))
                    {
                        throw new Exception();
                    }
                    Frame.BackStack.Clear();
                }

                ShowMessageAsync(ResourceLoader.GetForCurrentView("Resources").GetString("ErrorMessage"));
            }
            else
            {
                this.DefaultViewModel[Item] = DisplayedCategory;
                this.DefaultViewModel[Label] = DisplayedCategory.Label;

                if (DisplayedCategory.NextPageUri == null)
                {
                    this.Scroll.ViewChanged -= Scroll_ViewChanged;
                }

                this.CategoryPagePivot.Visibility = Visibility.Visible;
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

        private async void Scroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                var Sender = sender as ScrollViewer;
                if (Sender.ScrollableHeight - Sender.VerticalOffset == 0)
                {
                    var CurrentCategory = this.DefaultViewModel[Item] as Category;

                    if (CurrentCategory.NextPageUri == null)
                    {
                        this.Scroll.ViewChanged -= Scroll_ViewChanged;
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

                        bool isSuccess = await CategoriesSource.LoadMoreItemsAsync(CurrentCategory.Id, CurrentCategory.NextPageUri);

                        await statusBar.ProgressIndicator.HideAsync();
                        
                        if (isSuccess)
                        {
                            if ((this.DefaultViewModel[Item] as Category).NextPageUri == null)
                            {
                                this.Scroll.ViewChanged -= Scroll_ViewChanged;
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

        private void Course_Click(object sender, ItemClickEventArgs e)
        {
            var Id = ((Item)e.ClickedItem).Id;
            if (!Frame.Navigate(typeof(CoursePage), Id))
            {
                throw new Exception();
            }
        }
    }
}
