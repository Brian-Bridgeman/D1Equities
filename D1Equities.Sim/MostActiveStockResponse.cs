using System.Text.Json.Serialization;

namespace D1Equities.Sim
{
    public class MostActiveStockResponse
    {
        [JsonPropertyName("most_actives")]
        public List<MostActiveStock> MostActives { get; set; }
    }

    public class MostActiveStock
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("volume")]
        public long Volume { get; set; }

        [JsonPropertyName("trade_count")]
        public long TradeCount { get; set; }

        [JsonPropertyName("last_updated")]
        public DateTime LastUpdated { get; set; }
    }
}