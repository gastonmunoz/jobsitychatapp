using System;

namespace JobsityChatBotFunction.Exceptions
{
    /// <summary>
    /// Exception raised from the http request to stooq.com
    /// </summary>
    [Serializable]
    public class StooqUnavailableException : Exception
    {
        public StooqUnavailableException() 
            : base("Stooq.com unavailable")
        {
        }
    }
}
