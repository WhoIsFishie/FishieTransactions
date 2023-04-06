using FishieTransactions.Data;
using FishieTransactions.Helper;
using FishieTransactions.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Net.Http;

using static FishieTransactions.Data.Statics;

namespace FishieTransactions.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _contextAccessor;

        private readonly IHttpClientFactory _clientFactory;

        public ApiClient(HttpClient client, IHttpContextAccessor contextAccessor, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            //_client = client;
            _contextAccessor = contextAccessor;
            _client = _clientFactory.CreateClient();
        }

        /// <summary>
        /// login to the api
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ResponseCode.Code> LoginAsync(Cred input)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", input.Username),
                new KeyValuePair<string, string>("password", input.Password),
            });

            var response = await _client.PostAsync(Statics.URL + "login", formContent);

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    return await LoginProcessorAsync(await response.Content.ReadAsStringAsync());
                case System.Net.HttpStatusCode.BadGateway:
                    //means you done did fuck up big time bucko
                    return ResponseCode.Code.ProxyFail;
                default:
                    return ResponseCode.Code.unknown;

            }
        }

        private async Task<ResponseCode.Code> LoginProcessorAsync(string json)
        {
            var jObject = JObject.Parse(json);

            var result = (ResponseCode.Code)jObject.GetValue("code").ToString().ToInt32();

            switch (result)
            {
                case ResponseCode.Code.success:
                    return await GetUserInfoAsync();
                default:
                    return result;
            }
        }

        public async Task<ResponseCode.Code> GetUserInfoAsync()
        {
            //ping profile api so userinfo is allowed
            //idk why it is like this whoever made the bank api makes it so you have to do this
            //so ill just have to work with it

            //i have now grown as an individual and now undustand why you have to select profile
            //if you are not a business account owner you do not have to worry about this piece of code
            HttpResponseMessage ProfileInfoMsg = await _client.GetAsync(URL + @"profile");
            string ProfileInfoJson = await ProfileInfoMsg.Content.ReadAsStringAsync();

            JObject jObject1 = JObject.Parse(ProfileInfoJson);
            JToken Token = jObject1.SelectToken("payload").SelectToken("profile");
            var profiles = Token.FirstOrDefault();
            var id = profiles.SelectToken("profile").ToString();

            var formContent = new FormUrlEncodedContent(new[]
            {
             new KeyValuePair<string, string>("profile", id),
            });

            HttpResponseMessage responseMessage = await _client.PostAsync(URL + @"profile", formContent);

            var responseJson = await responseMessage.Content.ReadAsStringAsync();
            formContent.Dispose();

            var jObject = JObject.Parse(responseJson);
            var result = (ResponseCode.Code)jObject.GetValue("code").ToString().ToInt32();

            if (result != ResponseCode.Code.success)
            {
                return result;
            }

            //send a get request to bank servers to get userinfo
            HttpResponseMessage UserInfoMessage = await _client.GetAsync(URL + @"userinfo");
            string UserInfoJson = await UserInfoMessage.Content.ReadAsStringAsync();
            JObject jObject_ = JObject.Parse(UserInfoJson);
            var n = jObject_.GetValue("success").Value<string>();

            Console.WriteLine("User Info Details\n" + UserInfoJson);

            if (n.ToLower() == "false")
            {
                return ResponseCode.Code.success;
            }
            return (ResponseCode.Code)jObject_.GetValue("code").ToString().ToInt32();
        }

        public async Task<(ResponseCode.Code, List<SmallDashBoardClass>)> GetSimplifiedDashBoard()
        {
            HttpResponseMessage DashboardInfoMessage = await _client.GetAsync(URL + @"dashboard");
            string DashboardInfoJson = await DashboardInfoMessage.Content.ReadAsStringAsync();
            // string DashboardInfoJsonFixed = DashboardInfoJson.Substring(1, DashboardInfoJson.Length - 2);
            JObject jObject = JObject.Parse(DashboardInfoJson);

            var code = jObject.SelectToken("code");

            ResponseCode.Code responseCode = (ResponseCode.Code)code.ToString().ToInt32();
            if (responseCode != ResponseCode.Code.success)
            {
                return (responseCode, null);
            }

            JToken Token = jObject.SelectToken("payload").SelectToken("dashboard");

            Console.WriteLine("GetSimplifiedDashBoard Details\n" + DashboardInfoJson);

            List<SmallDashBoardClass> account_data = new List<SmallDashBoardClass>();

            foreach (var item in Token.Children())
            {
                SmallDashBoardClass temp = new SmallDashBoardClass();

                var account = item.SelectToken("account");
                temp.accounts = account.ToString();

                var id = item.SelectToken("id");
                temp.id = id.ToString();

                //this fix is in case bank doesnt return an alias. in such a case
                //use title instead. sometimes bank dont return title
                var alias = item.SelectToken("alias");
                if (alias == null)
                {
                    temp.alias = item.SelectToken("title").ToString();
                }
                else
                    temp.alias = alias.ToString();

                var product = item.SelectToken("product");
                temp.product = product.ToString();

                var currency = item.SelectToken("currency");
                temp.currency = currency.ToString();

                account_data.Add(temp);
            }

            return (responseCode, account_data);
        }

        public async Task<(ResponseCode.Code, List<HistoryObject>)> GetTodaysHistory(string id)
        {
            if (!await CheckIfLoggedIn())
            {
                await LoginAsync(Statics.loginDetails);
            }

            HttpResponseMessage accountHistoryInfoMessageInfoMessage = await _client.GetAsync(URL + $@"account/{id}/history/today");
            string accountHistoryInfoJson = await accountHistoryInfoMessageInfoMessage.Content.ReadAsStringAsync();
            if (!accountHistoryInfoJson.Contains("Invalid Account Number"))
            {
                //quick and dirty hack to remove some items
                AccountHistory m = JsonConvert.DeserializeObject<AccountHistory>(accountHistoryInfoJson);

                //instead of removing items from the list new method is to
                //make a copy and add items to the list and assign it to view
                AccountHistory copy = new AccountHistory();

                copy.message = m.message;
                copy.success = m.success;
                copy.code = m.code;
                copy.payload = new Payload();

                ResponseCode.Code responseCode = (ResponseCode.Code)m.code;

                if (responseCode != ResponseCode.Code.success)
                {
                    return (responseCode, null);
                }

                //we reverse the list to make the newest transections come to top
                m.payload.history.Reverse();
                return (responseCode, m.payload.history);
            }
            return (ResponseCode.Code.accountIDIssue, null);
        }

        /// <summary>
        /// checks to see if the user is logged into bank or not
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckIfLoggedIn()
        {
            await _client.GetAsync(URL + "profile");
            HttpResponseMessage UserInfoMessage = await _client.GetAsync(URL + "userinfo");
            string UserInfoJson = await UserInfoMessage.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(UserInfoJson);

            var n = jObject.GetValue("success").Value<string>();
            if (n.ToLower() == "false")
            {
                return false;
            }
            return true;
        }
    }
}

