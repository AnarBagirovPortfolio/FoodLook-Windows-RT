using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;

namespace FoodLook_2.DataModel
{
    class SearchAnswer
    {
        public SearchAnswer()
        {
            this.Next = null;

            this.SearchResults = new ObservableCollection<SearchResult>();
        }

        public SearchAnswer(string jsonString)
        {
            JsonObject Data = JsonObject.Parse(jsonString);

            this.Next = (Data.GetNamedValue("next").ValueType == JsonValueType.String) ? new Uri(Data.GetNamedString("next")) : null;
            this.SearchResults = new ObservableCollection<SearchResult>();

            foreach (var item in Data.GetNamedArray("results").Select(n => n.GetObject()))
            {
                int Id = item.ContainsKey("restaurant") ? (int)item.GetNamedObject("restaurant").GetNamedNumber("id") : (int)item.GetNamedNumber("id");
                int LocationId = item.ContainsKey("restaurant") ? (int)item.GetNamedNumber("id") : -1;
                double Latitude = item.ContainsKey("latitude") ? item.GetNamedNumber("latitude") : -1;
                double Longitude = item.ContainsKey("longitude") ? item.GetNamedNumber("longitude") : -1;
                string Telephones = item.ContainsKey("telephones") ? item.GetNamedString("telephones") : null;
                string Address = item.ContainsKey("address") ? item.GetNamedString("address") : null;
                string Label = item.ContainsKey("restaurant") ? item.GetNamedObject("restaurant").GetNamedString("label") : item.GetNamedString("label");

                this.SearchResults.Add(new SearchResult(Id, Label, new Location(LocationId, Latitude, Longitude, Address, Telephones)));
            }
        }

        public Uri Next { get; set; }
        public ObservableCollection<SearchResult> SearchResults { get; private set; }
    }

    class SearchResult
    {
        public SearchResult(int id, string label, Location itemlocation)
        {
            this.Id = id;
            this.Label = label;
            this.ItemLocation = itemlocation;
        }

        public int Id { get; private set; }
        public string Label { get; private set; }
        public Location ItemLocation { get; private set; }
    }

    class SearchSource
    {
        private static SearchSource _searchSource = new SearchSource();

        private ObservableCollection<SearchAnswer> _answers = new ObservableCollection<SearchAnswer>();
        public ObservableCollection<SearchAnswer> Answers
        {
            get { return this._answers; }
        }

        public static async Task<SearchAnswer> GetSearchAnswer(Uri SearchUri)
        {
            string JsonDataString = await Api.GetJsonDataAsync(SearchUri);

            if (JsonDataString == null)
            {
                return null;
            }
            else
            {
                _searchSource.Answers.Clear();

                var Answer = new SearchAnswer(JsonDataString);

                _searchSource.Answers.Add(Answer);

                return Answer;
            }
        }

        public static async Task<bool> LoadMoreItemsAsync(Uri NextPageUri)
        {
            string JsonDataString = await Api.GetJsonDataAsync(NextPageUri);

            if (JsonDataString == null)
            {
                return false;
            }
            else
            {
                var NewAnswer = new SearchAnswer(JsonDataString);

                _searchSource.Answers[0].Next = NewAnswer.Next;

                foreach (var item in NewAnswer.SearchResults)
                {
                    _searchSource.Answers[0].SearchResults.Add(item);
                }

                return true;
            }
        }
    }
}
