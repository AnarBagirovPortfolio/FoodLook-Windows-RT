namespace FoodLook_2.DataModel
{
    public class Social
    {
        public Social(int comments, int likeid, int likes, int shares, int favoriteid)
        {
            this.Comments = comments;
            this.LikeId = likeid;
            this.Likes = likes;
            this.Shares = shares;
            this.FavoriteId = favoriteid;
        }

        public int Comments { get; set; }
        public int LikeId { get; set; }
        public int Likes { get; set; }
        public int Shares { get; set; }
        public int FavoriteId { get; set; }
    }

    public class MenuItem
    {
        public MenuItem(int id, string label)
        {
            this.Id = id;
            this.Label = label;
        }

        public int Id { get; private set; }
        public string Label { get; private set; }
    }

    public class Photo
    {
        public Photo(string photopath)
        {
            this.PhotoPath = photopath;
        }

        public string PhotoPath { get; private set; }
        public double PhotoSize { get { return (Windows.UI.Xaml.Window.Current.Bounds.Width - 2 * 19 - 3 * 12) / 4; } }
    }

    public class Location
    {
        public Location(int id, double latitude, double longitude, string address, string telephones)
        {
            this.Id = id;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Address = address;
            this.Telephones = telephones;
        }

        public int Id { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public string Address { get; private set; }
        public string Telephones { get; private set; }
    }

    public class UserInfo
    {
        public UserInfo(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
    }
}