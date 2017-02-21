using FoodLook_2.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;

namespace FoodLook_2.DataModel
{
    public class Category
    {
        public Category(string id, string label, Uri next)
        {
            this.Id = id;
            this.Label = label;
            this.NextPageUri = next;
            this.Items = new ObservableCollection<Item>();
        }

        public string Id { get; private set; }
        public string Label { get; private set; }
        public Uri NextPageUri { get; set; }
        public ObservableCollection<Item> Items { get; private set; }
    }

    class CategoriesSource
    {
        public static CategoriesSource _categoriesSource = new CategoriesSource();

        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        public ObservableCollection<Category> Categories
        {
            get { return this._categories; }
        }

        public static async Task<Category> GetCategoryAsync(string id)
        {
            if (id.Contains(':'))
            {
                var matches = _categoriesSource.Categories.Where(n => n.Id == id);
                if (matches.Count() == 0)
                {
                    int RestaurantId = Convert.ToInt32(id.Split(':').First());
                    int CategoryId = Convert.ToInt32(id.Split(':').Last());

                    Uri RequestUri = null;

                    if (RestaurantId == -1)
                    {
                        RequestUri = new Uri("https://www.foodlook.az/api/categories/" + CategoryId.ToString() + "/courses/");
                    }
                    else
                    {
                        RequestUri = new Uri("https://www.foodlook.az/api/restaurants/" + RestaurantId.ToString() + "/categories/" + CategoryId.ToString() + "/");
                    }

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
                            var CategoriesGroup = await MainPageSource.GetGroupAsync(MainPage.CategoriesPivotItemName);
                            if (CategoriesGroup == null)
                            {
                                return null;
                            }
                            else
                            {
                                var CategoryItem = CategoriesGroup.Items.Where(n => n.Id == CategoryId);
                                if (CategoryItem.Count() == 0)
                                {
                                    return null;
                                }
                                else
                                {
                                    JsonObject JsonData = JsonObject.Parse(JsonDataString);

                                    Uri NextPageUri = (JsonData.GetNamedValue("next").ValueType == JsonValueType.String) ? new Uri(JsonData.GetNamedString("next")) : null;

                                    Category NewCategory = new Category(id, CategoryItem.First().Label, NextPageUri);

                                    foreach (var item in JsonData.GetNamedArray("results").Select(n => n.GetObject()))
                                    {
                                        int Id = (int)item.GetNamedNumber("id");
                                        string Label = item.GetNamedString("label");
                                        string ImagePath = item.GetNamedString("image"); ;
                                        string Description = ResourceLoader.GetForCurrentView("Resources").GetString("Price") + " " + item.GetNamedNumber("price").ToString() + " AZN"; ;

                                        NewCategory.Items.Add(new Item(Id, Label, ImagePath, Description));
                                    }

                                    _categoriesSource.Categories.Add(NewCategory);

                                    return NewCategory;
                                }
                            }
                        }
                    }
                }
                else
                {
                    return matches.First();
                }
            }
            else
            {
                return null;
            }
        }

        public static async Task<bool> LoadMoreItemsAsync(string id, Uri nextpageuri)
        {
            var matches = _categoriesSource.Categories.Where(n => n.Id == id);
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                if (nextpageuri == null)
                {
                    return false;
                }
                else
                {
                    string JsonDataString = await Api.GetJsonDataAsync(nextpageuri);

                    if (JsonDataString == null)
                    {
                        return false;
                    }
                    else
                    {
                        JsonObject JsonData = JsonObject.Parse(JsonDataString);

                        int matchIndex = _categoriesSource.Categories.IndexOf(matches.First());

                        Uri newNextPageUri = (JsonData.GetNamedValue("next").ValueType == JsonValueType.String) ? new Uri(JsonData.GetNamedString("next")) : null;

                        _categoriesSource.Categories[matchIndex].NextPageUri = newNextPageUri;

                        foreach (var item in JsonData.GetNamedArray("results").Select(n => n.GetObject()))
                        {
                            int Id = (int)item.GetNamedNumber("id");
                            string Label = item.GetNamedString("label");
                            string ImagePath = item.GetNamedString("image"); ;
                            string Description = ResourceLoader.GetForCurrentView("Resources").GetString("Price") + " " + item.GetNamedNumber("price").ToString() + " AZN"; ;

                            _categoriesSource.Categories[matchIndex].Items.Add(new Item(Id, Label, ImagePath, Description));
                        }

                        return true;
                    }
                }                
            }
        }
    }
}
