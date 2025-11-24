using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace D1Equities.Sim
{
    public class Portfolio
    {
        public string? UserId { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalEquity => Balance + GetTotalPositionsValue();
        public Dictionary<string, Position> Positions { get; set; } = [];
        public List<EquityHistory> EquityHistory { get; set; } = [];

        [JsonIgnore]
        private string? PortfolioPath { get; set; }
        public decimal GetTotalPositionsValue() => Positions.Values.Select(p => p.CurentValue).Sum();
        public decimal GetTotalPortfolioValueChange() => TotalEquity - EquityHistory.First().Equity;

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
            File.WriteAllText(PortfolioPath!, JsonSerializer.Serialize(this));
        }

        public void BuyShares(string symbol, decimal price, int quantity)
        {
            //TODO - skapa Position och l'gg till i Positions eller l
            if (string.IsNullOrWhiteSpace(symbol)) 
                throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));
            if (price <= 0m) 
                throw new ArgumentException(nameof(price));
            if (quantity <= 0)
                throw new ArgumentException(nameof(quantity));

            decimal cost = price * quantity;
            if (cost > Balance)
                throw new Exception("Insufficient balance to open position.");

            Balance -= cost;

            if (Positions.TryGetValue(symbol, out var existing))
            {
                existing.AddShares(quantity, price);
            }
            else
            {
                var position = new Position(symbol, quantity, price);
                Positions[symbol] = position;
            }

            EquityHistory.Add(new EquityHistory(DateTime.Now, TotalEquity));

        }


        public void SellShares(string symbol, decimal price, int quantity)
        {
            if (!Positions.TryGetValue(symbol, out var pos))
                throw new Exception($"No position for {symbol}");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            if (quantity > pos.Shares)
                throw new Exception("Cannot sell more shares than owned");

            pos.RemoveShares(quantity);
            Balance += quantity * price;

            // Only remove if zero shares remain
            if (pos.Shares == 0)
                Positions.Remove(symbol);

            EquityHistory.Add(new EquityHistory(DateTime.Now, TotalEquity));
        }

    }
}
