namespace JobsityChatBotFunction.Classes
{
    /// <summary>
    /// Class for Azure service bus queue 
    /// </summary>
    public class StockMessage
    {
        public string Message { get;set; }
        public string GroupName { get; set; }
    }
}
