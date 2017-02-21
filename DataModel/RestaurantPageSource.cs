using FoodLook_2.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.Storage;

namespace FoodLook_2.DataModel
{
    public class Restaurant
    {
        public Restaurant(string JsonDataString)
        {
            JsonObject JsonData = JsonObject.Parse(JsonDataString);

            this.Id = (int)JsonData.GetNamedNumber("id");
            this.Label = JsonData.GetNamedString("label");
            this.Description = JsonData.GetNamedString("description");
            this.Cuisine = JsonData.GetNamedString("cuisine");
            this.Telephone = (JsonData.GetNamedValue("telephone").ValueType == JsonValueType.String) ? JsonData.GetNamedString("telephone") : null;

            this.LiveMusic = JsonData.GetNamedBoolean("live_music");
            this.PaymentCards = JsonData.GetNamedBoolean("payment_cards");
            this.Parking = JsonData.GetNamedBoolean("parking");

            this.OperationTime = "Ресторан" + " " + JsonData.GetNamedString("restaurant_open") + " - " + JsonData.GetNamedString("restaurant_open") + "\n" +
                                 "Кухня" + " " + JsonData.GetNamedString("kitchen_open") + " - " + JsonData.GetNamedString("kitchen_close");
            this.Logo = JsonData.GetNamedString("logo");
            this.BackgroundImage = JsonData.GetNamedString("background_image");

            this.Email = (JsonData.GetNamedValue("email").ValueType == JsonValueType.String) ? JsonData.GetNamedString("email") : null;
            this.Website = (JsonData.GetNamedValue("website").ValueType == JsonValueType.String) ? JsonData.GetNamedString("website") : null;
            this.Facebook = (JsonData.GetNamedValue("facebook").ValueType == JsonValueType.String) ? JsonData.GetNamedString("facebook") : null;
            this.Instagram = (JsonData.GetNamedValue("instagram").ValueType == JsonValueType.String) ? JsonData.GetNamedString("instagram") : null;

            this.Locations = new ObservableCollection<Location>();
            foreach (var item in JsonData.GetNamedArray("locations").Select(n => n.GetObject()))
            {
                Locations.Add(new Location((int)item.GetNamedNumber("id"),
                                           (item.GetNamedValue("latitude").ValueType == JsonValueType.Number) ? item.GetNamedNumber("latitude") : -1,
                                           (item.GetNamedValue("longitude").ValueType == JsonValueType.Number) ? item.GetNamedNumber("longitude") : -1,
                                           item.GetNamedString("address"),
                                           item.GetNamedString("telephones")));
            }

            this.Photos = new ObservableCollection<Photo>();
            foreach (var item in JsonData.GetNamedArray("photos").Select(n => n.GetString()))
            {
                Photos.Add(new Photo(item));
            }

            this.Menu = new ObservableCollection<MenuItem>();
            foreach (var item in JsonData.GetNamedArray("menu").Select(n => n.GetObject()))
            {
                int Id = (int)item.GetNamedNumber("id");
                string Label = item.GetNamedString("label");

                Menu.Add(new MenuItem(Id, Label));
            }

            this.RestaurantSocial = new Social(
                (int)JsonData.GetNamedObject("social").GetNamedNumber("comments"),
                (JsonData.GetNamedObject("social").GetNamedValue("like_id").ValueType == JsonValueType.Number) ? (int)JsonData.GetNamedObject("social").GetNamedNumber("like_id") : -1,
                (int)JsonData.GetNamedObject("social").GetNamedNumber("likes"),
                (int)JsonData.GetNamedObject("social").GetNamedNumber("shares"),
                (JsonData.GetNamedObject("social").GetNamedValue("favourite_id").ValueType == JsonValueType.Number) ? (int)JsonData.GetNamedObject("social").GetNamedNumber("favourite_id") : -1);
        }

        public int Id { get; private set; }
        public string Label { get; private set; }
        public string Description { get; private set; }
        public string Cuisine { get; private set; }
        public string Telephone { get; private set; }
        public bool LiveMusic { get; private set; }
        public bool PaymentCards { get; private set; }
        public bool Parking { get; private set; }
        public string OperationTime { get; private set; }
        public string Logo { get; private set; }
        public string BackgroundImage { get; private set; }
        public string Email { get; private set; }
        public string Website { get; private set; }
        public string Facebook { get; private set; }
        public string Instagram { get; private set; }
        public ObservableCollection<Location> Locations { get; private set; }
        public ObservableCollection<Photo> Photos { get; private set; }
        public ObservableCollection<MenuItem> Menu { get; private set; }
        public Social RestaurantSocial { get; private set; }
    }

    class RestaurantPageSource
    {
        private static RestaurantPageSource _restaurantPageSource = new RestaurantPageSource();

        private ObservableCollection<Restaurant> _restaurants = new ObservableCollection<Restaurant>();
        public ObservableCollection<Restaurant> Restaurants
        {
            get { return this._restaurants; }
        }

        public static async Task<Restaurant> GetRestaurantAsync(int id)
        {
            var matches = _restaurantPageSource.Restaurants.Where((restaurant) => restaurant.Id.Equals(id));
            if (matches.Count() == 0)
            {
                Uri RequestUri = new Uri("https://www.foodlook.az/api/restaurants/" + id.ToString() + "/");

                string JsonDataString = await Api.GetJsonDataAsync(RequestUri);

                if (JsonDataString == null)
                {
                    return null;
                }
                else
                {
                    Restaurant NewRestaurant = new Restaurant(JsonDataString);

                    _restaurantPageSource.Restaurants.Add(NewRestaurant);

                    return NewRestaurant;
                }
            }
            else
            {
                return matches.First();
            }
        }

        public static async Task<bool> AddLikeAsync(int id)
        {
            var matches = _restaurantPageSource.Restaurants.Where((restaurant) => restaurant.Id.Equals(id));
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                string JsonDataString = await Api.AddLikeAsync(id);

                if (JsonDataString == null)
                {
                    return false;
                }
                else
                {
                    var matchIndex = _restaurantPageSource.Restaurants.IndexOf(matches.First());

                    _restaurantPageSource.Restaurants[matchIndex].RestaurantSocial.LikeId = (int)JsonObject.Parse(JsonDataString).GetNamedNumber("id");

                    return true;
                }
            }
        }

        public static async Task<bool> RemoveLikeAsync(int id)
        {
            var matches = _restaurantPageSource.Restaurants.Where((restaurant) => restaurant.Id.Equals(id));
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                var matchIndex = _restaurantPageSource.Restaurants.IndexOf(matches.First());

                int LikeId = _restaurantPageSource.Restaurants[matchIndex].RestaurantSocial.LikeId;

                if (LikeId == -1)
                {
                    return false;
                }
                else
                {
                    bool isDeleteSuccess = await Api.RemoveLikeAsync(LikeId);

                    if (isDeleteSuccess)
                    {
                        _restaurantPageSource.Restaurants[matchIndex].RestaurantSocial.LikeId = -1;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public static async Task<bool> AddToFavoriteAsync(int id)
        {
            var matches = _restaurantPageSource.Restaurants.Where((restaurant) => restaurant.Id.Equals(id));
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                string JsonDataString = await Api.AddToFavoriteAsync(id);

                if (JsonDataString == null)
                {
                    return false;
                }
                else
                {
                    var matchIndex = _restaurantPageSource.Restaurants.IndexOf(matches.First());

                    _restaurantPageSource.Restaurants[matchIndex].RestaurantSocial.FavoriteId = (int)JsonObject.Parse(JsonDataString).GetNamedNumber("id");

                    MainPageSource.AddToFavorite(MainPage.RestaurantsPivotItemName, MainPage.FavoriteRestaurantsPivotItemName, id);

                    return true;
                }
            }
        }

        public static async Task<bool> RemoveFromFavoriteAsync(int id)
        {
            var matches = _restaurantPageSource.Restaurants.Where((restaurant) => restaurant.Id.Equals(id));
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                var matchIndex = _restaurantPageSource.Restaurants.IndexOf(matches.First());

                var FavoriteId = _restaurantPageSource.Restaurants[matchIndex].RestaurantSocial.FavoriteId;

                if (FavoriteId == -1)
                {
                    return false;
                }
                else
                {
                    bool isSuccess = await Api.RemoveFromFavoriteAsync(FavoriteId);

                    if (isSuccess)
                    {
                        _restaurantPageSource.Restaurants[matchIndex].RestaurantSocial.FavoriteId = -1;

                        MainPageSource.RemoveFromFavorite(MainPage.FavoriteRestaurantsPivotItemName, id);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
