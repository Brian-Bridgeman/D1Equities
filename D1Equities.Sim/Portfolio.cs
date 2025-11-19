using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace D1Equities.Sim
{
    public class Portfolio
    {
        public string? UserId { get; init; }
        public decimal Balance { get; init; }
        public Dictionary<string, Position> Positions { get; } = [];
        public List<EquityHistory> EquityHistory { get; init; } = [];

        [JsonIgnore]
        private string? PortfolioPath { get; set; }

        public static Portfolio Load(string userId)
        {
            var portfolioDir = Path.Combine(".", "portfolios");

            if (!Directory.Exists(portfolioDir))
                Directory.CreateDirectory(portfolioDir);

            var portfolioFile = Path.Combine(portfolioDir, $"{userId}.json");

            if (!File.Exists(portfolioFile))
            {
                var newPortfolio = new Portfolio
                {
                    UserId = userId,
                    Balance = 10_000M,
                    PortfolioPath = portfolioFile,
                    EquityHistory = [new EquityHistory(DateTime.Now, 10_000M)]
                };

                File.WriteAllText(portfolioFile, JsonSerializer.Serialize(newPortfolio));
                return newPortfolio;
            }

            try
            {
                var json = File.ReadAllText(portfolioFile);

                var portfolio = JsonSerializer.Deserialize<Portfolio>(json)
                    ?? throw new Exception("Couldn't deserialize portfolio file");

                portfolio.PortfolioPath = portfolioFile;
                return portfolio;
            }
            catch (JsonException e)
            {
                throw new Exception($"Portfolio file for user '{userId}' is corrupted.", e);
            }
            catch (Exception e)
            {
                throw new Exception("An unknown error occurred while loading the portfolio.", e);
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
