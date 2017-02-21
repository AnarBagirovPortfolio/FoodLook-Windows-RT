using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;

namespace FoodLook_2.DataModel
{
    class Api
    {
        public static CancellationTokenSource cancelationTokenSource = new CancellationTokenSource();

        public static Windows.Storage.ApplicationDataContainer Settings = Windows.Storage.ApplicationData.Current.RoamingSettings;
        private const string UserTokenKey = "UserToken";
        private const string RefreshTokenKey = "RefreshToken";
        private const string CreationTimeKey = "TokenCreationTime";
        public const string Username = "Username";
        public const string Password = "Password";

        private const string ClientId = "";
        private const string ClientSecret = "";
        private const string UserId = "";
        private const string UserSecret = "";

        private static string UserToken = Settings.Values.ContainsKey(UserTokenKey) ? Settings.Values[UserTokenKey].ToString() : null;
        private static string RefreshToken = Settings.Values.ContainsKey(RefreshTokenKey) ? Settings.Values[RefreshTokenKey].ToString() : null;
        private static DateTime TokenCreationTime = Settings.Values.ContainsKey(CreationTimeKey) ? DateTime.Parse(Settings.Values[CreationTimeKey].ToString()) : new DateTime();

        private static object TokenUpdateHelper = new object();
        private static bool TokenUpdateInProgress = false;

        private static async Task GetTokenAsync()
        {
            DateTime RequestDateTime = DateTime.UtcNow;

            Uri RequestUri = new Uri("https://www.foodlook.az/api/oauth2/token/");
            var RequestContent = new StringContent(UserId + "&" + UserSecret + "&grant_type=refresh_token&refresh_token=" + RefreshToken, Encoding.UTF8, "application/x-www-form-urlencoded");

            string ServerAnswer = null;

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded; charset=utf-8");
                    var responce = await httpClient.PostAsync(RequestUri, RequestContent);
                    ServerAnswer = await responce.Content.ReadAsStringAsync();
                }
            }
            catch { }         
   
            if (ServerAnswer != null)
            {
                if (JsonObject.Parse(ServerAnswer).ContainsKey("access_token"))
                {
                    UserToken = JsonObject.Parse(ServerAnswer).GetNamedString("access_token");
                    RefreshToken = JsonObject.Parse(ServerAnswer).GetNamedString("refresh_token");
                    TokenCreationTime = RequestDateTime;

                    //Save received data
                    Settings.Values[UserTokenKey] = UserToken;
                    Settings.Values[RefreshTokenKey] = RefreshToken;
                    Settings.Values[CreationTimeKey] = TokenCreationTime.ToString();
                }
            }            
        }

        public static async Task<string> GetTokenAsync(string username, string password)
        {
            DateTime RequestDateTime = DateTime.UtcNow;

            Uri RequestUri = new Uri("https://www.foodlook.az/api/oauth2/token/");
            var RequestContent = new StringContent(UserId + "&" + UserSecret + "&grant_type=password&username=" + username + "&password=" + password, Encoding.UTF8, "application/x-www-form-urlencoded");

            string ServerAnswer = String.Empty;

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded; charset=utf-8");
                    var responce = await httpClient.PostAsync(RequestUri, RequestContent);
                    ServerAnswer = await responce.Content.ReadAsStringAsync();
                }
            }
            catch { }

            if (ServerAnswer.Equals(String.Empty))
            {
                return "NetworkError";
            }
            else
            {
                if (JsonObject.Parse(ServerAnswer).ContainsKey("access_token"))
                {
                    UserToken = JsonObject.Parse(ServerAnswer).GetNamedString("access_token");
                    RefreshToken = JsonObject.Parse(ServerAnswer).GetNamedString("refresh_token");
                    TokenCreationTime = RequestDateTime;

                    //Save received data
                    Settings.Values[UserTokenKey] = UserToken;
                    Settings.Values[RefreshTokenKey] = RefreshToken;
                    Settings.Values[CreationTimeKey] = TokenCreationTime.ToString();

                    return "Okay";
                }
                else
                {
                    return "ExistError";
                }
            }            
        }

        private static async Task UpdateTokenAsync()
        {
            lock(TokenUpdateHelper)
            {
                if (TokenUpdateInProgress)
                {
                    SpinWait.SpinUntil(() => !TokenUpdateInProgress);
                    return;
                }
                else
                {
                    TokenUpdateInProgress = true;
                }
            }

            if (UserToken == null)
            {
                if (RefreshToken == null)
                {
                    await GetTokenAsync(Settings.Values[Username].ToString(), Settings.Values[Password].ToString());
                }
                else
                {
                    await GetTokenAsync();
                }
            }
            else if (TimeSpan.Compare(DateTime.UtcNow.Subtract(TokenCreationTime), new TimeSpan(24, 0, 0)) > 0)
            {
                if (RefreshToken == null)
                {
                    await GetTokenAsync(Settings.Values[Username].ToString(), Settings.Values[Password].ToString());
                }
                else
                {
                    await GetTokenAsync();
                }
            }

            lock(TokenUpdateHelper)
            {
                TokenUpdateInProgress = false;
            }
        }

        public static async Task<string> GetJsonDataAsync(Uri requesturi)
        {
            await UpdateTokenAsync();

            string JsonDataString = null;

            if (UserToken != null)
            {

                try
                {
                    using (HttpClient myClient = new HttpClient())
                    {
                        myClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + UserToken);
                        myClient.DefaultRequestHeaders.Add("ContentType", "application/json; charset=utf-8");
                        myClient.DefaultRequestHeaders.Add("Accept-Language", ResourceLoader.GetForCurrentView("Resources").GetString("Language"));
                        var responce = await myClient.GetAsync(requesturi, cancelationTokenSource.Token);
                        JsonDataString = await responce.Content.ReadAsStringAsync();
                    }
                }
                catch
                {

                }
            }

            return JsonDataString;
        }

        public static async Task<string> AddLikeAsync(int id)
        {
            await UpdateTokenAsync();

            string JsonDataString = null;

            if (UserToken != null)
            {
                JsonObject Body = new JsonObject();
                Body.SetNamedValue("entity", JsonValue.CreateStringValue(id.ToString()));

                var Content = new StringContent(Body.Stringify(), Encoding.UTF8, "application/json");

                try
                {
                    using (HttpClient myClient = new HttpClient())
                    {
                        myClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + UserToken);
                        myClient.DefaultRequestHeaders.Add("ContentType", "application/json; charset=utf-8");
                        var responce = await myClient.PostAsync(new Uri("https://www.foodlook.az/api/likes/"), Content);
                        JsonDataString = await responce.Content.ReadAsStringAsync();
                    }
                }
                catch
                {

                }
            }

            return JsonDataString;
        }

        public static async Task<bool> RemoveLikeAsync(int likeid)
        {
            await UpdateTokenAsync();

            bool isSuccess = false;

            if (UserToken != null)
            {
                Uri RequestUri = new Uri("https://www.foodlook.az/api/likes/" + likeid + "/");

                try
                {
                    using (HttpClient myClient = new HttpClient())
                    {
                        myClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + UserToken);
                        await myClient.DeleteAsync(RequestUri);
                    }

                    isSuccess = true;
                }
                catch
                {
                    
                }
            }

            return isSuccess;
        }

        public static async Task<string> AddToFavoriteAsync(int id)
        {
            await UpdateTokenAsync();

            string JsonDataString = null;

            if (UserToken != null)
            {
                JsonObject Body = new JsonObject();
                Body.SetNamedValue("entity", JsonValue.CreateStringValue(id.ToString()));

                var Content = new StringContent(Body.Stringify(), Encoding.UTF8, "application/json");

                try
                {
                    using (HttpClient myClient = new HttpClient())
                    {
                        myClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + UserToken);
                        myClient.DefaultRequestHeaders.Add("ContentType", "application/json; charset=utf-8");
                        var responce = await myClient.PostAsync(new Uri("https://www.foodlook.az/api/favourites/"), Content);
                        JsonDataString = await responce.Content.ReadAsStringAsync();
                    }
                }
                catch
                {

                }
            }

            return JsonDataString;
        }

        public static async Task<bool> ChangePasswordAsync(string oldpassword, string newpassword)
        {
            await UpdateTokenAsync();

            bool isSuccess = false;

            if (UserToken != null)
            {
                JsonObject Body = new JsonObject();
                Body.SetNamedValue("old_password", JsonValue.CreateStringValue(oldpassword));
                Body.SetNamedValue("new_password", JsonValue.CreateStringValue(newpassword));

                var Content = new StringContent(Body.Stringify(), Encoding.UTF8, "application/json");

                try
                {
                    using (HttpClient myClient = new HttpClient())
                    {
                        myClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + UserToken);
                        myClient.DefaultRequestHeaders.Add("ContentType", "application/json; charset=utf-8");
                        await myClient.PutAsync(new Uri("https://www.foodlook.az/api/users/me/"), Content);
                        isSuccess = true;
                    }
                }
                catch
                {

                }
            }

            return isSuccess;
        }

        public static async Task<bool> RemoveFromFavoriteAsync(int favoriteidid)
        {
            await UpdateTokenAsync();

            bool isSuccess = false;

            if (UserToken != null)
            {
                Uri RequestUri = new Uri("https://www.foodlook.az/api/favourites/" + favoriteidid + "/");

                try
                {
                    using (HttpClient myClient = new HttpClient())
                    {
                        myClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + UserToken);
                        await myClient.DeleteAsync(RequestUri);
                    }

                    isSuccess = true;
                }
                catch
                {

                }
            }

            return isSuccess;
        }

        public static async Task<List<object>> AddCommentAsync(int entity, string text)
        {
            await UpdateTokenAsync();

            List<object> Data = new List<object>();

            if (UserToken != null)
            {
                string UserInfoString = await GetUserInfoStringAsync();

                if (UserInfoString != null)
                {
                    UserInfo Userinfo = new UserInfo((int)JsonObject.Parse(UserInfoString).GetNamedNumber("id"), JsonObject.Parse(UserInfoString).GetNamedString("username"));

                    JsonObject Body = new JsonObject();
                    Body.SetNamedValue("entity", JsonValue.CreateStringValue(entity.ToString()));
                    Body.SetNamedValue("text", JsonValue.CreateStringValue(text));

                    var Content = new StringContent(Body.Stringify(), Encoding.UTF8, "application/json");

                    string CommentInfo = null;

                    try
                    {
                        using (HttpClient myClient = new HttpClient())
                        {
                            myClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + UserToken);
                            myClient.DefaultRequestHeaders.Add("ContentType", "application/json; charset=utf-8");
                            var responce = await myClient.PostAsync(new Uri("https://www.foodlook.az/api/comments/"), Content);

                            CommentInfo = await responce.Content.ReadAsStringAsync();

                            Data.Add(Userinfo);
                            Data.Add(CommentInfo);
                        }
                    }
                    catch
                    {

                    }
                }                
            }

            return Data;
        }

        public static async Task<bool> RemoveCommentAsync(int id)
        {
            await UpdateTokenAsync();

            bool isSuccess = false;

            if (UserToken != null)
            {
                Uri RequestUri = new Uri("https://www.foodlook.az/api/comments/" + id + "/");

                try
                {
                    using (HttpClient myClient = new HttpClient())
                    {
                        myClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + UserToken);
                        await myClient.DeleteAsync(RequestUri);
                    }

                    isSuccess = true;
                }
                catch
                {

                }
                
            }

            return isSuccess;
        }

        public static async Task<bool> ChangeCommentAsync(int id, string newtext)
        {
            await UpdateTokenAsync();

            bool isSuccess = false;

            if (UserToken != null)
            {
                Uri RequestUri = new Uri("https://www.foodlook.az/api/comments/" + id + "/");

                JsonObject Body = new JsonObject();
                Body.SetNamedValue("text", JsonValue.CreateStringValue(newtext));
                var Content = new StringContent(Body.Stringify(), Encoding.UTF8, "application/json");

                try
                {
                    using (HttpClient myClient = new HttpClient())
                    {
                        myClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + UserToken);
                        myClient.DefaultRequestHeaders.Add("ContentType", "application/json; charset=utf-8");
                        await myClient.PutAsync(RequestUri, Content);
                    }

                    isSuccess = true;
                }
                catch
                {

                }
            }

            return isSuccess;
        }

        public static async Task<string> GetUserInfoStringAsync()
        {
            await UpdateTokenAsync();

            string JsonDataString = null;

            if (UserToken != null)
            {
                Uri RequestUri = new Uri("https://www.foodlook.az/api/users/me/");

                try
                {
                    using (HttpClient myClient = new HttpClient())
                    {
                        myClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + UserToken);
                        myClient.DefaultRequestHeaders.Add("ContentType", "application/json; charset=utf-8");
                        var responce = await myClient.GetAsync(RequestUri);

                        JsonDataString = await responce.Content.ReadAsStringAsync();
                    }
                }
                catch
                {

                }
            }

            return JsonDataString;
        }

        public static async Task<string> RegisterUserAsync(string username, string password)
        {
            string Token = String.Empty;
            Uri TokenUri = new Uri("https://www.foodlook.az/api/oauth2/token/");
            var TokenContent = new StringContent(ClientId + "&" + ClientSecret + "&grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            try
            {
                using (HttpClient myClient = new HttpClient())
                {
                    myClient.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded; charset=utf-8");
                    var responce = await myClient.PostAsync(TokenUri, TokenContent);
                    string ServerAnswer = await responce.Content.ReadAsStringAsync();
                    Token = JsonObject.Parse(ServerAnswer).GetNamedString("access_token");
                }
            }
            catch { }

            if (Token.Equals(String.Empty))
            {
                return "NetworkError";
            }
            else
            {
                Uri RegistrationUri = new Uri("https://www.foodlook.az/api/users/register/");

                JsonObject Body = new JsonObject();
                Body.SetNamedValue("username", JsonValue.CreateStringValue(username));
                Body.SetNamedValue("password", JsonValue.CreateStringValue(password));

                var RegistrationContent = new StringContent(Body.Stringify(), Encoding.UTF8, "application/json");
                string ResponceMessageString = String.Empty;

                try
                {
                    using (HttpClient myClient = new HttpClient())
                    {
                        myClient.DefaultRequestHeaders.Add("ContentType", "application/json; charset=utf-8");
                        myClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + Token);
                        HttpResponseMessage ResponceMessage = await myClient.PostAsync(RegistrationUri, RegistrationContent);
                        ResponceMessageString = await ResponceMessage.Content.ReadAsStringAsync();
                    }
                }
                catch { }

                if (ResponceMessageString.Equals(String.Empty))
                {
                    return "NetworkError";
                }
                else
                {
                    if (JsonObject.Parse(ResponceMessageString).GetNamedValue("username").Stringify().Contains("exist"))
                    {
                        return "ExistError";
                    }
                    else
                    {
                        return "Okay";
                    }
                }                
            }
        }
    }
}
