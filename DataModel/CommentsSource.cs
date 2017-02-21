using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace FoodLook_2.DataModel
{
    class CommentItem
    {
        public CommentItem(int id, string created, string lastupdated, string text, UserInfo user)
        {
            this.Id = id;
            this.Created = DateTime.Parse(created).ToString();
            this.LastUpdated = DateTime.Parse(lastupdated).ToString();
            this.Text = text;
            this.User = user;
        }

        public int Id { get; private set; }
        public string Created { get; private set; }
        public string LastUpdated { get; set; }
        public string Text { get; set; }
        public UserInfo User { get; private set; }
    }

    class CommentsGroup
    {
        public CommentsGroup(int id, string jsondatastring)
        {
            this.Id = id;

            JsonObject JsonData = JsonObject.Parse(jsondatastring);

            this.NextPageUri = (JsonData.GetNamedValue("next").ValueType == JsonValueType.String) ? new Uri(JsonData.GetNamedString("next")) : null;
            this.Comments = new ObservableCollection<CommentItem>();

            foreach (var item in JsonData.GetNamedArray("results").Select(n => n.GetObject()))
            {
                int Id = (int)item.GetNamedNumber("id");
                string Created = item.GetNamedString("created");
                string LastUpdated = item.GetNamedString("last_updated");
                string Text = item.GetNamedString("text");
                UserInfo User = new UserInfo((int)item.GetNamedObject("user").GetNamedNumber("id"), item.GetNamedObject("user").GetNamedString("username"));

                this.Comments.Add(new CommentItem(Id, Created, LastUpdated, Text, User));
            }
        }

        public int Id { get; private set; }
        public Uri NextPageUri { get; set; }
        public ObservableCollection<CommentItem> Comments { get; private set; }
    }

    class CommentsSource
    {
        private static CommentsSource _commentsSource = new CommentsSource();

        private ObservableCollection<CommentsGroup> _comments = new ObservableCollection<CommentsGroup>();
        public ObservableCollection<CommentsGroup> Comments
        {
            get { return this._comments; }
        }

        public static async Task<CommentsGroup> GetCommentsAsync(int id)
        {
            var matches = _commentsSource.Comments.Where(n => n.Id == id);
            if (matches.Count() == 0)
            {
                Uri RequestUri = new Uri("https://www.foodlook.az/api/comments/list/" + id.ToString() + "/?order=asc");

                string JsonDataString = await Api.GetJsonDataAsync(RequestUri);

                if (JsonDataString == null)
                {
                    return null;
                }
                else
                {
                    var NewComments = new CommentsGroup(id, JsonDataString);

                    _commentsSource.Comments.Add(NewComments);

                    return NewComments;
                }
            }
            else
            {
                return matches.First();
            }
        }

        public static async Task<bool> AddCommentAsync(int id, string text)
        {
            var matches = _commentsSource.Comments.Where(n => n.Id == id);
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                List<object> Data = await Api.AddCommentAsync(id, text);

                if (Data.Count() == 0)
                {
                    return false;
                }
                else
                {
                    if (Data.Count() == 2)
                    {
                        UserInfo User = Data.First() as UserInfo;
                        string CommentInfo = Data.Last() as string;
                        JsonObject JsonData = JsonObject.Parse(CommentInfo);

                        int Id = (int)JsonData.GetNamedNumber("id");
                        string Text = JsonData.GetNamedString("text");

                        var NewComment = new CommentItem(Id, DateTime.Now.ToString(), DateTime.Now.ToString(), Text, User);

                        var matchIndex = _commentsSource.Comments.IndexOf(matches.First());
                        _commentsSource.Comments[matchIndex].Comments.Insert(0, NewComment);

                        return true;
                    }
                    else
                    {
                        return false;
                    }                    
                }
            }
        }

        public static async Task<bool> RemoveCommentAsync(int entity, int id)
        {
            var matches = _commentsSource.Comments.Where(n => n.Id == entity);
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                var matchIndex = _commentsSource.Comments.IndexOf(matches.First());

                var Comment = _commentsSource.Comments[matchIndex].Comments.Where(n => n.Id == id);
                if (Comment.Count() == 0)
                {
                    return false;
                }
                else
                {
                    bool isSuccess = await Api.RemoveCommentAsync(id);

                    if (isSuccess)
                    {
                        _commentsSource.Comments[matchIndex].Comments.Remove(Comment.First());

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }                
            }
        }

        public static async Task<bool> ChangeCommentAsync(int entity, int id, string newtext)
        {
            var matches = _commentsSource.Comments.Where(n => n.Id == entity);
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                var matchIndex = _commentsSource.Comments.IndexOf(matches.First());

                var Comment = _commentsSource.Comments[matchIndex].Comments.Where(n => n.Id == id);
                if (Comment.Count() == 0)
                {
                    return false;
                }
                else
                {
                    bool isSuccess = await Api.ChangeCommentAsync(id, newtext);

                    if (isSuccess)
                    {
                        var CommentIndex = _commentsSource.Comments[matchIndex].Comments.IndexOf(Comment.First());

                        var NewComment = _commentsSource.Comments[matchIndex].Comments[CommentIndex];
                        NewComment.Text = newtext;
                        NewComment.LastUpdated = DateTime.Now.ToString();

                        _commentsSource.Comments[matchIndex].Comments.RemoveAt(CommentIndex);
                        _commentsSource.Comments[matchIndex].Comments.Insert(CommentIndex, NewComment);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public static async Task<bool> LoadMoreCommentsAsync(int id)
        {
            var matches = _commentsSource.Comments.Where(n => n.Id == id);
            if (matches.Count() == 0)
            {
                return false;
            }
            else
            {
                Uri RequestUri = matches.First().NextPageUri;

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
                        var NewComments = new CommentsGroup(id, JsonDataString);

                        var GroupIndex = _commentsSource.Comments.IndexOf(matches.First());

                        foreach (var comment in NewComments.Comments)
                        {
                            if (_commentsSource.Comments[GroupIndex].Comments.Where(n => n.Id == comment.Id).Count() == 0)
                            {
                                _commentsSource.Comments[GroupIndex].Comments.Add(comment);
                            }                            
                        }

                        _commentsSource.Comments[GroupIndex].NextPageUri = NewComments.NextPageUri;

                        return true;
                    }
                }
            }
        }
    }
}
