using CsvHelper.Configuration.Attributes;

namespace JobsityChatBot.Classes
{
    /// <summary>
    /// Class for mapping the stooq.com response
    /// </summary>
    public class Stock
    {
        [Name("Symbol")]
        public string Symbol { get; set; }
        [Name("Date")]
        public string Date { get; set; }
        [Name("Time")]
        public string Time { get; set; }
        [Name("Open")]
        public double Open { get; set; }
        [Name("High")]
        public double High { get; set; }
        [Name("Low")]
        public double Low { get; set; }
        [Name("Close")]
        public double Close { get; set; }
        [Name("Volume")]
        public long Volume { get; set; }
    }
}
