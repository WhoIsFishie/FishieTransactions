using FishieTransactions.Helper;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FishieTransactions.Models
{
    public class AccountHistory
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public Payload payload { get; set; }
    }



    public class Payload
    {
        public List<HistoryObject> history { get; set; }
    }

    [BsonCollection("history")]
    public class HistoryObject : Document
    {
        [JsonProperty("id")]
        public string History_id { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("reference")]
        public string reference { get; set; }

        [JsonProperty("currency")]
        public string currency { get; set; }

        [JsonProperty("amount")]
        public float amount { get; set; }

        [JsonProperty("balance")]
        public float balance { get; set; }

        [JsonProperty("narrative1")]
        public string narrative1 { get; set; }

        [JsonProperty("narrative2")]
        public string date { get; set; }

        [JsonProperty("narrative3")]
        public string name { get; set; }

        [JsonProperty("narrative4")]
        public string narrative4 { get; set; }

        [JsonProperty("minus")]
        public bool minus { get; set; }


        //turn to false once a transection has been credited
        /// <summary>
        /// true means the payment can be credited
        /// false means its already been credited 
        /// </summary>
        public bool valid { get; set; } = true;

        //creates a hash of the transection in case bank fucks up the ref or id
        public string SelfHash { get; set; }
    }
}
