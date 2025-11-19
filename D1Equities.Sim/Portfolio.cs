using System.Text.Json;
using System.Text.Json.Serialization;

namespace D1Equities.Sim
{
    public class Portfolio
    {
        public string? UserId { get; init; }
        public decimal Balance { get; set; } = 10_000M;
        public decimal TotalEquity { get; set; }
        public Dictionary<string, Position> Positions { get; set; } = [];
        public EquityHistory[] EquityHistory { get; set; } = [];

        [JsonIgnore]
        private string? PortfolioPath { get; set; }

        public static Portfolio? Load(string userId)
        {
            var portfolioDir = Path.Combine(".", "portfolios");

            if (!Directory.Exists(portfolioDir))
                Directory.CreateDirectory(portfolioDir);

            var portfolioFile = Path.Combine(portfolioDir, $"{userId}.json");

            if (!File.Exists(portfolioFile))
                File.Create(portfolioFile);

            try
            {
                var portfolio = JsonSerializer.Deserialize<Portfolio>(File.ReadAllText(portfolioFile))
                    ?? throw new Exception("Couldn't deserialize portfolio file");

                portfolio.PortfolioPath = portfolioFile;

                return portfolio;
            }
            catch (JsonException)
            {
                throw new Exception($"Portfolio file for user '{userId}' is corrupted.");
            }
            catch(Exception e)
            {
                throw new Exception("An unknown error occured", e);
            }
        }

        public void Save()
        {
            File.WriteAllText(PortfolioPath, JsonSerializer.Serialize(this));
        }

        public void OpenPosition(string symbol, decimal price, int quantity)
        {
            //TODO - skapa Position och l'gg till i Positions eller l
        }

        public void ClosePosition(string symbol)
        {
            var hasPos = Positions.TryGetValue(symbol, out var pos);
            if (!hasPos)
                throw new Exception($"Cant close position in {symbol} because it doesnt exist");

            //TODO - l's v'rdet av pos och ta bort ur dict och l'gg till v'rde pa balance
        }
    }
}
