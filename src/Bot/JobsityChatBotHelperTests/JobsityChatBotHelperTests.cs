using JobsityChatBotFunction.Classes;
using JobsityChatBotFunction.Exceptions;
using JobsityChatBotFunction.Helpers;
using System.Text;

namespace JobsityChatBotHelperTests
{
    [TestClass]
    public class JobsityChatBotHelperTests
    {
        private HttpClient httpClient;

        [TestInitialize]
        public void Initialize()
        {
            DelegatingHandlerStub handlerStub = new();
            httpClient = new HttpClient(handlerStub);
        }

        [TestMethod]
        [ExpectedException(typeof(IncorrectSintaxException), "Please use the format: \"/stock=stock_code\"")]
        public void Test_GetStockName_WithIncorrectCommand_Fail()
        {
            JobsityChatBotHelper.GetStockName("hello world!");
        }

        [TestMethod]
        [ExpectedException(typeof(IncorrectSintaxException), "Please use the format: \"/stock=stock_code\"")]
        public void Test_GetStockName_WithIncorrectCommands_Fail()
        {
            JobsityChatBotHelper.GetStockName("/stock=google");
        }

        [TestMethod]
        public void Test_GetStockName_WithCorrectCommand_Ok()
        {
            JobsityChatBotHelper.GetStockName("/stock=googl.us");
        }

        [TestMethod]
        public async Task Test_GetStockValues_WithCorrectStock_Ok()
        {
            MemoryStream response = await JobsityChatBotHelper.GetStockValues("googl.us", httpClient);
            string str = Encoding.Default.GetString(response.ToArray()).Replace("\n", "").Replace("\r", "");
            Assert.AreEqual(str, "Symbol,Date,Time,Open,High,Low,Close,VolumeAAPL.US,2022-09-29,22:00:07,146.1,146.72,140.68,142.48,127772886");
        }

        [TestMethod]
        public void Test_GetStockFromApi_WithCorrectStock_Ok()
        {
            Stock stock = JobsityChatBotHelper.GetStockFromApi("aapl.us", httpClient);
            Assert.AreEqual(stock.High, 146.72);
            Assert.AreEqual(stock.Low, 140.68);
            Assert.AreEqual(stock.Date, "2022-09-29");
            Assert.AreEqual(stock.Close, 142.48);
            Assert.AreEqual(stock.Symbol, "AAPL.US");
            Assert.AreEqual(stock.Open, 146.1);
            Assert.AreEqual(stock.Volume, 127772886);
            Assert.AreEqual(stock.Time, "22:00:07");
        }

        [TestMethod]
        public void Test_GetStockFromApi_WithInCorrectStock_Ok()
        {
            Stock stock = JobsityChatBotHelper.GetStockFromApi("googl.us", httpClient);
            Assert.IsNull(stock);
        }
    }
}