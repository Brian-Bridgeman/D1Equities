using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace D1Equities.Sim
{
    public class MarketMoversResponse
    {
        [JsonPropertyName("gainers")]
        public List<MoverItem> Gainers { get; set; } = new();

        [JsonPropertyName("losers")]
        public List<MoverItem> Losers { get; set; } = new();

        [JsonPropertyName("market_type")]
        public string MarketType { get; set; } = string.Empty;

        [JsonPropertyName("last_updated")]
        public DateTime LastUpdated { get; set; }
    }

    public class MoverItem
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("percent_change")]
        public double PercentChange { get; set; }

        [JsonPropertyName("change")]
        public double Change { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }
    }
}
