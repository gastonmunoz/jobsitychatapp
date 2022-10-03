using CsvHelper;
using JobsityChatBotFunction.Classes;
using JobsityChatBotFunction.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JobsityChatBotFunction.Helpers
{
    /// <summary>
    /// Get the stock name from the user command
    /// </summary>
    /// <param name="message">User command</param>
    /// <returns>Stock name</returns>
    public static class JobsityChatBotHelper
    {
        public static string GetStockName(string message)
        {
            string[] parts = message.Split("=");
            if (parts.Length >= 2)
            {
                foreach (string word in parts)
                {
                    if (Regex.Match(word, "\\w+[.]\\w+").Success)
                    {
                        return word;
                    }
                }
                throw new IncorrectSintaxException();
            }
            throw new IncorrectSintaxException();
        }

        /// <summary>
        /// Ask stooq.com for a stock value
        /// </summary>
        /// <param name="stock">Stock name</param>
        /// <returns></returns>
        /// <exception cref="StooqUnavailableException">Http exception</exception>
        public static async Task<MemoryStream> GetStockValues(string stock, HttpClient httpClient)
        {
            Dictionary<string, string> values = new();
            FormUrlEncodedContent content = new(values);
            try
            {
                using HttpResponseMessage response = await httpClient.PostAsync($"https://stooq.com/q/l/?s={stock}&f=sd2t2ohlcv&h&e=csv", content);
                byte[] responseContent = await response.Content.ReadAsByteArrayAsync();
                return new MemoryStream(responseContent);
            }
            catch (Exception)
            {
                throw new StooqUnavailableException();
            }
        }

        /// <summary>
        /// Calls the stooq.com API and parse the CSV to a Stock object
        /// </summary>
        /// <param name="stock">Stock name</param>
        /// <returns>Stock object</returns>
        public static Stock GetStockFromApi(string stock, HttpClient httpClient)
        {
            using StreamReader reader = new(GetStockValues(stock, httpClient).Result);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
            IEnumerable<Stock> records = csv.GetRecords<Stock>();
            return records.FirstOrDefault(p => p.Symbol.ToLower() == stock);
        }
    }
}
