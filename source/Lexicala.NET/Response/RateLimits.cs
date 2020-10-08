namespace Lexicala.NET.Response
{
    public class RateLimits
    {
        public int DailyLimit { get; set; }
        public int DailyLimitRemaining { get; set; }
        public int Limit { get; set; }
        public int Remaining { get; set; }
    }
}
