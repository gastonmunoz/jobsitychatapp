using System;

namespace JobsityChatBotFunction.Exceptions
{
    /// <summary>
    /// Exception raised from an incorrect user input
    /// </summary>
    [Serializable]
    public class IncorrectSintaxException : Exception
    {
        public IncorrectSintaxException() 
            : base("Please use the format: \"/stock=stock_code\"")
        {
        }
    }
}