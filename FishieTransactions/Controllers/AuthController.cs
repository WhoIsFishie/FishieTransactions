using DnsClient.Protocol;
using FishieTransactions.Data;
using FishieTransactions.Models;
using FishieTransactions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.InteropServices;
using static MongoDB.Driver.WriteConcern;
using System.Security.Cryptography.Xml;
using System.Reflection.PortableExecutable;

namespace FishieTransactions.Controllers
{
    [Route("api/bank")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IMongoRepository<HistoryObject> _history;
        private readonly IApiClient _apiClient;


        public AuthController(IApiClient apiClient, IMongoRepository<HistoryObject> history)
        {
            _apiClient = apiClient;
            _history = history;
        }

        /// <summary>
        /// login to the users bank account
        /// </summary>
        /// <param name="input">login details</param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(Cred input)
        {
            var result = await _apiClient.LoginAsync(input);

            switch (result)
            {
                case ResponseCode.Code.success:
                    Statics.SaveLoginDetails(input);
                    return Ok();
                default:
                    return StatusCode(((int)result));
            }
        }

        [HttpGet("login")]
        public async Task<IActionResult> GetLogin()
        {
            if (Statics.loginDetails != null)
            {
                var result = await _apiClient.LoginAsync(Statics.loginDetails);

                switch (result)
                {
                    case ResponseCode.Code.success:
                        return Ok();
                    default:
                        return StatusCode(((int)result));
                }
            }

            return Problem("Login creds missing");
        }

        /// <summary>
        /// select which accounts to use for when scanning for payments
        /// </summary>
        /// <returns></returns>
        [HttpPost("selectAccounts")]
        public async Task<IActionResult> SelectAccounts(List<string> accounts)
        {
            //TODO: add validation to make sure selected account id is present in the actual account list
            Statics.SaveAccounts(accounts);
            return Ok();
        }

        [HttpGet("getAccounts")]
        public async Task<IActionResult> GetAccounts()
        {
            return Ok(JsonConvert.SerializeObject(await _apiClient.GetSimplifiedDashBoard()));

        }

        [HttpGet("getSelectedAccounts")]
        public async Task<IActionResult> GetSelectedAccounts()
        {
            return Ok(JsonConvert.SerializeObject(Statics.Accounts));
        }

        /// <summary>
        /// setting the base url for making api calls
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpPost("setUrl")]
        public async Task<IActionResult> SetUrl(string url)
        {
            Statics.SaveUrl(url);
            return Ok();
        }

        [HttpGet("getHistory")]
        public async Task<IActionResult> GetHistory()
        {
            if (Statics.loginDetails == null)
            {
                return Problem("Login Creds missing");
            }

            try
            {
                List<HistoryObject> final = new();
                foreach (var item in Statics.Accounts)
                {
                    (var result, var output) = await _apiClient.GetTodaysHistory(item);
                    if (result == ResponseCode.Code.success)
                    {
                        switch (result)
                        {
                            case ResponseCode.Code.success:
                                //add filter to make sure only incoming payments are processed 
                                foreach (var history in output.Where(x => x.minus == false))
                                {
                                    //generate a self hash of the payment. in an ideal world this would be done
                                    //in a similer system to a blockchain where the hash also contains the hash of the
                                    //previous payment but i cant figure out how to do that so we have to settle with a 
                                    //self hash for now. while this method is pretty safe for avoiding clashes
                                    //if the balance gets deducted in the perfact amount it could cause 
                                    //the user to be credited twice 

                                    //md5 hash is used since its fast. if someone have an issue with it feel free
                                    //to submit a ticket
                                    using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                                    {
                                        //we use the balance as the most important value to keep track of
                                        //timeline when it comes to giving each transection a unique ID
                                        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(history.narrative4 + history.narrative1 + history.balance.ToString() + history.amount.ToString() + history.reference);
                                        byte[] hashBytes = md5.ComputeHash(inputBytes);

                                        history.SelfHash = Convert.ToHexString(hashBytes);
                                    }
                                    final.Add(history);

                                    //check if any payment hash is already in the db
                                    var payment = _history.FilterBy(x => x.SelfHash == history.SelfHash).FirstOrDefault();

                                    //if not in db then enter to the db
                                    if (payment == null)
                                    {
                                        await _history.InsertOneAsync(history);
                                    }
                                    break;
                                }
                                
                                ///this is a test implimentation of a blockchain based system to keep
                                ///track of transections to avoid double spending
                                ///each of the payment is hashed based on the previous payment
                                ///thus making it so that the likelyhood of a clashing transection drasticly dropping
                                //var history_ = output.Where(x => x.minus == false).ToList();
                                //for (int i = 0; i < history_.Count(); i++)
                                //{
                                //    using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                                //    {
                                //        byte[] inputBytes;
                                //        if (i != 0) //if this isnt the 0th item then use the hash of the previous payments hash
                                //            inputBytes = System.Text.Encoding.ASCII.GetBytes(history_[i].narrative4 + history_[i].narrative1 + history_[i].balance.ToString() + history_[i].amount.ToString() + history_[i].reference + history_[i - 1].SelfHash);
                                //        else
                                //            inputBytes = System.Text.Encoding.ASCII.GetBytes(history_[i].narrative4 + history_[i].narrative1 + history_[i].balance.ToString() + history_[i].amount.ToString() + history_[i].reference);
                                //        byte[] hashBytes = md5.ComputeHash(inputBytes);

                                //        history_[i].SelfHash = Convert.ToHexString(hashBytes);
                                //    }
                                //    final.Add(history_[i]);
                                //    var payment = _history.FilterBy(x => x.SelfHash == history_[i].SelfHash).FirstOrDefault();
                                //    if (payment == null)
                                //    {
                                //        await _history.InsertOneAsync(history_[i]);
                                //    }
                                //}

                                break;
                            default:
                                return StatusCode(((int)result));
                        }
                    }
                }

                return Ok(JsonConvert.SerializeObject(final));

            }
            catch (Exception ex)
            {
                return Problem("Unknown error while trying to get history");
            }
        }

        /// <summary>
        /// The method performs the following operations:
        ///It filters the records in the collection "_history" to find a matching record where "amount" matches "Amount", "name" matches "Name", "date" matches today's date, and "valid" is set to true.
        ///If a matching record is not found, the method calls the "GetHistory" method and returns a "404 Not Found" response.
        ///If a matching record is found, it sets the "valid" property to "false".
        ///It then updates the corresponding document in the "_history" collection by setting "valid" to "false".
        ///Finally, it returns an "200 OK" response with a JSON serialization of the "result" object.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        [HttpPost("confirmPayment")]
        public async Task<IActionResult> ConfirmPayment(string Name, float Amount)
        {
            var result = _history.FilterBy(x => x.amount == Amount && x.name == Name && x.date == DateTime.Today.ToString("dd-MM-yyyy") && x.valid == true).FirstOrDefault();

            if (result == null)
            {
                await GetHistory();

                return NotFound();
            }
            result.valid = false;
            var filter = Builders<HistoryObject>.Filter.Eq("_id", result.Id);
            var update = Builders<HistoryObject>.Update
                .Set("valid", false);
            await _history.UpdateOneAsync(filter, update);


            return Ok(JsonConvert.SerializeObject(result));
        }

        [HttpPost("unconfirmPayment")]
        public async Task<IActionResult> UnconfirmPayment(string hash)
        {
            var result = _history.FilterBy(x => x.SelfHash == hash).FirstOrDefault();

            var filter = Builders<HistoryObject>.Filter.Eq("_id", result.Id);
            var update = Builders<HistoryObject>.Update
                .Set("valid", true);
            await _history.UpdateOneAsync(filter, update);

            return Ok();
        }

        [HttpGet("GetTodayPayment")]
        public async Task<IActionResult> GetTodayPayment()
        {
            //_history.FilterBy(x => x.date)
            return Ok();
        }
    }
}
