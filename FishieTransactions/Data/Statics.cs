using FishieTransactions.Helper;
using FishieTransactions.Models;
using Newtonsoft.Json;

namespace FishieTransactions.Data
{
    public static class Statics
    {
        private static readonly IConfiguration Configuration;

        /// <summary>
        /// baseurl address to make api calls to
        /// </summary>
        public static string URL { get; set; }
        public static Cred loginDetails { get; set; }
        public static List<string> Accounts = new List<string>();
        public static List<SmallDashBoardClass> DashBoard = new List<SmallDashBoardClass>();

        public static string key;

        public static void GetUrlFromFile()
        {
            if (File.Exists("url.txt"))
            {
                URL = File.ReadAllText("url.txt");
            }
            else
            {
                throw new Exception("URL NOT SET");
            }
        }

        /// <summary>
        /// set the static url and also write it to file so next time server starts up
        /// user wont have to manually type in the url
        /// </summary>
        /// <param name="url"></param>
        public static void SaveUrl(string url)
        {
            File.WriteAllText("url.txt", url);
            URL = url;
        }

        public static void GetAccountsFromFile()
        {
            if (File.Exists("acc.txt"))
            {
                var list = File.ReadAllLines("acc.txt");
                Accounts.Clear();
                foreach (var item in list)
                {
                    Accounts.Add(item);
                }
            }
            else
            {
                throw new Exception("ACCOUNT NOT SET");
            }
        }

        public static void SaveAccounts(List<string> acc)
        {
            File.WriteAllLines("acc.txt", acc);
            Accounts = acc;
        }

        public static void LoadLoginDetails()
        {
            if (File.Exists("login.txt"))
            {
                var data = File.ReadAllText("login.txt");
                //var decryptionKey = key;
                //var output = SecurityManager.DecryptBytesToString(data, decryptionKey);
                loginDetails = JsonConvert.DeserializeObject<Cred>(data);
            }
            else
            {
                throw new Exception("Login cred unavailable");
            }
        }

        public static void SaveLoginDetails(Cred input)
        {
            var json = JsonConvert.SerializeObject(input);
            ///as you can see i did make efforts to encrypt the login cred but
            ///i coudlt get it to work so here we are with unenc login details
            ///if anyone have an issue with this feel free to submit a fix
            var decryptionKey = key;
            //byte[] encrypted = SecurityManager.EncryptStringToBytes(json, decryptionKey);

            File.WriteAllText("login.txt", json);

            loginDetails = input;
        }
    }
}
