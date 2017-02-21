using FoodLook_2.DataModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI.Xaml;

namespace FoodLook_2.Data
{
    public class Item
    {
        public Item(int id, String label, String imagePath, String description)
        {
            this.Id = id;
            this.Label = label;
            this.Description = description;
            this.ImagePath = imagePath;
        }

        public int Id { get; private set; }
        public string Label { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public double ImageSize { get { return (Window.Current.Bounds.Width - 2 * 19 - 12) / 2; } }
    }

    public class Group
    {
        public Group(String uniqueId, Uri nextpageuri)
        {
            this.UniqueId = uniqueId;
            this.NextPageUri = nextpageuri;
            this.Items = new ObservableCollection<Item>();
        }

        public string UniqueId { get; private set; }
        public Uri NextPageUri { get; set; }
        public ObservableCollection<Item> Items { get; private set; }
    }

    public sealed class MainPageSource
    {
        private static MainPageSource _mainPageSource = new MainPageSource();

        private ObservableCollection<Group> _groups = new ObservableCollection<Group>();
        public ObservableCollection<Group> Groups
        {
            get { return this._groups; }
        }

        public static async Task<Group> GetGroupAsync(string uniqueId)
        {
            var matches = _mainPageSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 0)
            {
                Uri RequestUri = GetGroupUri(uniqueId);

                if (RequestUri == null)
                {
                    return null;
                }
                else
                {
                    string JsonDataString = await Api.GetJsonDataAsync(RequestUri);

                    if (JsonDataString == null)
                    {
                        return null;
                    }
                    else
                    {
                        JsonObject JsonData = JsonObject.Parse(JsonDataString);

                        Uri NextPageUri = (JsonData.GetNamedValue("next").ValueType == JsonValueType.String) ? new Uri(JsonData.GetNamedString("next")) : null;

                        Group NewGroup = new Group(uniqueId, NextPageUri);

                        foreach (var item in JsonData.GetNamedArray("results").Select(n => n.GetObject()))
                        {
                            int Id = (int)item.GetNamedNumber("id");
                            string Label = item.GetNamedString("label");
                            string ImagePath = null;
                            string Description = null;

                            if (uniqueId == MainPage.RestaurantsPivotItemName || uniqueId == MainPage.PromotedRestaurantsPivotItemName || uniqueId == MainPage.FavoriteRestaurantsPivotItemName)
                            {
                                ImagePath = item.GetNamedString("logo");
                            }
                            else if (uniqueId == MainPage.CoursesPivotItemName || uniqueId == MainPage.VegetariaCoursesPivotItemName || uniqueId == MainPage.FavoriteCoursesPivotItemName)
                            {
                                ImagePath = item.GetNamedString("image");
                                Description = ResourceLoader.GetForCurrentView("Resources").GetString("Price") + " " + item.GetNamedNumber("price").ToString() + " AZN";
                            }

                            NewGroup.Items.Add(new Item(Id, Label, ImagePath, Description));
                        }

                        _mainPageSource.Groups.Add(NewGroup);

                        return NewGroup;
                    }
                }
            }
            else
            {
                return matches.First();
            }
        }

        public static async Task<bool> LoadMoreItemsAsync(string uniqueId)
        {
            var matches = _mainPageSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                var matchIndex = _mainPageSource.Groups.IndexOf(matches.First());
                Uri RequestUri = _mainPageSource.Groups[matchIndex].NextPageUri;

                if (RequestUri == null)
                {
                    return false;
                }
                else
                {
                    string JsonDataString = await Api.GetJsonDataAsync(RequestUri);

                    if (JsonDataString == null)
                    {
                        return false;
                    }
                    else
                    {
                        JsonObject JsonData = JsonObject.Parse(JsonDataString);
                        _mainPageSource.Groups[matchIndex].NextPageUri = (JsonData.GetNamedValue("next").ValueType == JsonValueType.String) ? new Uri(JsonData.GetNamedString("next")) : null;

                        foreach (var item in JsonData.GetNamedArray("results").Select(n => n.GetObject()))
                        {
                            int Id = (int)item.GetNamedNumber("id");
                            string Label = item.GetNamedString("label");
                            string ImagePath = null;
                            string Description = null;

                            if (uniqueId == MainPage.RestaurantsPivotItemName || uniqueId == MainPage.PromotedRestaurantsPivotItemName || uniqueId == MainPage.FavoriteRestaurantsPivotItemName)
                            {
                                ImagePath = item.GetNamedString("logo");
                            }
                            else if (uniqueId == MainPage.CoursesPivotItemName || uniqueId == MainPage.VegetariaCoursesPivotItemName || uniqueId == MainPage.FavoriteCoursesPivotItemName)
                            {
                                ImagePath = item.GetNamedString("image");
                                Description = ResourceLoader.GetForCurrentView("Resources").GetString("Price") + " " + item.GetNamedNumber("price").ToString() + " AZN";
                            }

                            _mainPageSource.Groups[matchIndex].Items.Add(new Item(Id, Label, ImagePath, Description));
                        }

                        return true;
                    }
                }
            }
        }

        public static Uri GetGroupUri(string uniqueId)
        {
            if (uniqueId == MainPage.RestaurantsPivotItemName) return new Uri("https://www.foodlook.az/api/restaurants/");
            if (uniqueId == MainPage.PromotedRestaurantsPivotItemName) return new Uri("https://www.foodlook.az/api/restaurants/promoted/");
            if (uniqueId == MainPage.CoursesPivotItemName) return new Uri("https://www.foodlook.az/api/courses/");
            if (uniqueId == MainPage.VegetariaCoursesPivotItemName) return new Uri("https://www.foodlook.az/api/courses/vegetarian/");
            if (uniqueId == MainPage.CategoriesPivotItemName) return new Uri("https://www.foodlook.az/api/categories/");
            if (uniqueId == MainPage.FavoriteRestaurantsPivotItemName) return new Uri("https://www.foodlook.az/api/users/me/favourites/restaurants/");
            if (uniqueId == MainPage.FavoriteCoursesPivotItemName) return new Uri("https://www.foodlook.az/api/users/me/favourites/courses/");
            return null;
        }

        public static void AddToFavorite(string SenderId, string GroupId, int ItemId)
        {
            if (_mainPageSource.Groups.Count(n => n.UniqueId.Equals(GroupId)).Equals(1))
            {
                int GroupIndex = _mainPageSource.Groups.IndexOf(_mainPageSource.Groups.Where(n => n.UniqueId.Equals(GroupId)).First());
                int SenderIndex = _mainPageSource.Groups.IndexOf(_mainPageSource.Groups.Where(n => n.UniqueId.Equals(SenderId)).First());
                int ItemIndex = _mainPageSource.Groups[SenderIndex].Items.IndexOf(_mainPageSource.Groups[SenderIndex].Items.Where(n => n.Id.Equals(ItemId)).First());

                _mainPageSource.Groups[GroupIndex].Items.Add(_mainPageSource.Groups[SenderIndex].Items[ItemIndex]);
            }
        }

        public static void RemoveFromFavorite(string GroupId, int ItemId)
        {
            if (_mainPageSource.Groups.Count(n => n.UniqueId.Equals(GroupId)).Equals(1))
            {
                int GroupIndex = _mainPageSource.Groups.IndexOf(_mainPageSource.Groups.Where(n => n.UniqueId.Equals(GroupId)).First());
                int ItemIndex = _mainPageSource.Groups[GroupIndex].Items.IndexOf(_mainPageSource.Groups[GroupIndex].Items.Where(n => n.Id.Equals(ItemId)).First());

                _mainPageSource.Groups[GroupIndex].Items.RemoveAt(ItemIndex);
            }
        }
    }
}