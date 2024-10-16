using Newtonsoft.Json;
using ServiceStack.Redis;

namespace BitcoinPriceFetcher
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Kết nối Redis
            var redisManager = new RedisManagerPool("redis:6379");
            using (var client = redisManager.GetClient())
            {
                // Tên list Redis
                string redisListName = "bitcoin-prices";

                // Client HTTP để gọi API
                using var httpClient = new HttpClient();

                while (true)
                {
                    try
                    {
                        // Lấy giá Bitcoin từ API Binance
                        var response = await httpClient.GetAsync("https://api.binance.com/api/v3/ticker/price?symbol=BTCUSDT");
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStringAsync();

                        // Parse JSON và lấy giá trị
                        dynamic data = JsonConvert.DeserializeObject(content);
                        decimal bitcoinPrice = data.price;

                        // Thêm giá vào list Redis
                        client.PushItemToList(redisListName, bitcoinPrice.ToString());

                        Console.WriteLine($"Đã thêm giá Bitcoin: {bitcoinPrice} vào Redis lúc {DateTime.Now}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi: {ex.Message}");
                    }

                    // Đợi 30 giây
                    Thread.Sleep(10000);
                }
            }
        }
    }
}
