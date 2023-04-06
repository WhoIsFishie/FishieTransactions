using FishieTransactions.Data;
using FishieTransactions.Models;

namespace FishieTransactions.Services
{
    public interface IApiClient
    {
        Task<ResponseCode.Code> LoginAsync(Cred input);
        Task<ResponseCode.Code> GetUserInfoAsync();
        Task<(ResponseCode.Code, List<SmallDashBoardClass>)> GetSimplifiedDashBoard();
        Task<(ResponseCode.Code, List<HistoryObject>)> GetTodaysHistory(string id);
    }
}
