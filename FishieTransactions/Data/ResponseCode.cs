namespace FishieTransactions.Data
{
    public class ResponseCode
    {
        /// <summary>
        /// response codes given by bank api
        /// </summary>
        public enum Code
        {
            success = 0,
            fail = 2,

            /// <summary>
            /// all login to the acount must stop when this error is thrown           
            /// </summary>
            locked = 20,

            /// <summary>
            /// gigidi
            /// </summary>
            maintenance = 37,

            /// <summary>
            /// this is a debug code. not code thrown by bank
            /// </summary>
            accountIDIssue = 6969,

            /// <summary>
            /// and bank has detected this as an unauth application
            /// </summary>
            ProxyFail = 1020,

            /// <summary>
            /// this is a debug code. not code thrown by bank
            /// </summary>
            unknown = 10000000
        }

    }
}
