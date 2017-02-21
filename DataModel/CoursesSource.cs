using FoodLook_2.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;

namespace FoodLook_2.DataModel
{
    public class Course
    {
        public Course(string jsondatastring)
        {
            JsonObject JsonData = JsonObject.Parse(jsondatastring);

            this.Id = (int)JsonData.GetNamedNumber("id");
            this.Label = JsonData.GetNamedString("label");
            this.Price = ResourceLoader.GetForCurrentView("Resources").GetString("Price") + " " + JsonData.GetNamedNumber("price").ToString() + " AZN";
            this.Ingredients = JsonData.GetNamedString("ingredients");
            this.Vegetarian = JsonData.GetNamedBoolean("is_vegetarian");
            this.ImagePath = new Uri(JsonData.GetNamedString("image"));

            this.Categories = new ObservableCollection<MenuItem>();
            foreach (var item in JsonData.GetNamedArray("categories").Select(n => n.GetObject()))
            {
                this.Categories.Add(new MenuItem((int)item.GetNamedNumber("id"), item.GetNamedString("label").ToLower()));
            }

            this.CourseSocial = new Social(
                (int)JsonData.GetNamedObject("social").GetNamedNumber("comments"),
                (JsonData.GetNamedObject("social").GetNamedValue("like_id").ValueType == JsonValueType.Number) ? (int)JsonData.GetNamedObject("social").GetNamedNumber("like_id") : -1,
                (int)JsonData.GetNamedObject("social").GetNamedNumber("likes"),
                (int)JsonData.GetNamedObject("social").GetNamedNumber("shares"),
                (JsonData.GetNamedObject("social").GetNamedValue("favourite_id").ValueType == JsonValueType.Number) ? (int)JsonData.GetNamedObject("social").GetNamedNumber("favourite_id") : -1);

            this.RestaurantId = (int)JsonData.GetNamedObject("restaurant").GetNamedNumber("id");
            this.RestaurantLabel = JsonData.GetNamedObject("restaurant").GetNamedString("label");
            this.RestaurantLogoPath = new Uri(JsonData.GetNamedObject("restaurant").GetNamedString("logo"));
        }

        public int Id { get; private set; }
        public string Label { get; private set; }
        public string Price { get; private set; }
        public string Ingredients { get; private set; }
        public bool Vegetarian { get; private set; }
        public Uri ImagePath { get; private set; }
        public ObservableCollection<MenuItem> Categories { get; private set; }
        public Social CourseSocial { get; private set; }
        public int RestaurantId { get; private set; }
        public string RestaurantLabel { get; private set; }
        public Uri RestaurantLogoPath { get; private set; }
    }

    class CoursesSource
    {
        private static CoursesSource _coursesSource = new CoursesSource();

        private ObservableCollection<Course> _courses = new ObservableCollection<Course>();
        public ObservableCollection<Course> Courses
        {
            get { return this._courses; }
        }

        public static async Task<Course> GetCourseAsync(int id)
        {
            var matches = _coursesSource.Courses.Where(n => n.Id.Equals(id));
            if (matches.Count() == 0)
            {
                Uri RequestUri = new Uri("https://www.foodlook.az/api/courses/" + id.ToString() + "/");

                string JsonDataString = await Api.GetJsonDataAsync(RequestUri);

                if (JsonDataString == null)
                {
                    return null;
                }
                else
                {
                    Course NewCourse = new Course(JsonDataString);

                    _coursesSource.Courses.Add(NewCourse);

                    return NewCourse;
                }
            }
            else
            {
                return matches.First();
            }
        }

        public static async Task<bool> AddLikeAsync(int id)
        {
            var matches = _coursesSource.Courses.Where(n => n.Id.Equals(id));
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
                    var matchIndex = _coursesSource.Courses.IndexOf(matches.First());

                    _coursesSource.Courses[matchIndex].CourseSocial.LikeId = (int)JsonObject.Parse(JsonDataString).GetNamedNumber("id");

                    return true;
                }
            }
        }

        public static async Task<bool> RemoveLikeAsync(int id)
        {
            var matches = _coursesSource.Courses.Where(n => n.Id.Equals(id));
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                var matchIndex = _coursesSource.Courses.IndexOf(matches.First());

                int LikeId = _coursesSource.Courses[matchIndex].CourseSocial.LikeId;

                if (LikeId == -1)
                {
                    return false;
                }
                else
                {
                    bool isDeleteSuccess = await Api.RemoveLikeAsync(LikeId);

                    if (isDeleteSuccess)
                    {
                        _coursesSource.Courses[matchIndex].CourseSocial.LikeId = -1;
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
            var matches = _coursesSource.Courses.Where(n => n.Id.Equals(id));
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
                    var matchIndex = _coursesSource.Courses.IndexOf(matches.First());

                    _coursesSource.Courses[matchIndex].CourseSocial.FavoriteId = (int)JsonObject.Parse(JsonDataString).GetNamedNumber("id");

                    MainPageSource.AddToFavorite(MainPage.CoursesPivotItemName, MainPage.FavoriteCoursesPivotItemName, id);

                    return true;
                }
            }
        }

        public static async Task<bool> RemoveFromFavoriteAsync(int id)
        {
            var matches = _coursesSource.Courses.Where(n => n.Id.Equals(id));
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                var matchIndex = _coursesSource.Courses.IndexOf(matches.First());

                var FavoriteId = _coursesSource.Courses[matchIndex].CourseSocial.FavoriteId;

                if (FavoriteId == -1)
                {
                    return false;
                }
                else
                {
                    bool isSuccess = await Api.RemoveFromFavoriteAsync(FavoriteId);

                    if (isSuccess)
                    {
                        _coursesSource.Courses[matchIndex].CourseSocial.FavoriteId = -1;

                        MainPageSource.RemoveFromFavorite(MainPage.FavoriteCoursesPivotItemName, id);

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
