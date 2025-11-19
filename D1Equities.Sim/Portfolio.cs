using System.Text.Json;
using System.Text.Json.Serialization;

namespace D1Equities.Sim
{
    public class Portfolio
    {
        public string UserId { get; set; }
        public decimal Balance { get; set; } = 10_000M;
        public decimal TotalEquity { get; set; }
        public Dictionary<string, Position> Positions { get; set; } = [];
        public EquityHistory[] EquityHistory { get; set; } = [];

        [JsonIgnore]
        private string PortfolioPath { get; }

        public Portfolio(string userId)
        {
            UserId = userId;
            PortfolioPath = Path.Combine(".", "portfolios", $"{userId}.json");
        }

        public static Portfolio Load(string userId)
        {
            var portfolio = new Portfolio(userId);
            var portfolioPath = Path.Combine(".", "portfolios", $"{userId}.json");

            if (string.IsNullOrEmpty(portfolioPath))
                return portfolio;

            portfolio = JsonSerializer.Deserialize<Portfolio>(File.ReadAllText(portfolioPath));

            if (portfolio == null)
                throw new Exception("Couldnt deserialize portfolio file");

            return portfolio;
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
