using System.Text.Json;
using System.Text.Json.Serialization;

namespace D1Equities.Sim
{
    public class HistoricalBarsResponse
    {
        [JsonPropertyName("bars")]
        public Dictionary<string, List<CandleStick>> Bars { get; init; } = [];
    }

    public class CandleStick
    {
        [JsonPropertyName("S")]
        public string Symbol { get; set; } = "";

        [JsonPropertyName("t")]
        public DateTime DateTime { get; set; }

        [JsonPropertyName("o")]
        public decimal Open { get; set; }

        [JsonPropertyName("h")]
        public decimal High { get; set; }

        [JsonPropertyName("l")]
        public decimal Low { get; set; }

        [JsonPropertyName("c")]
        public decimal Close { get; set; }

        [JsonPropertyName("v")]
        public long Volume { get; set; }

        [JsonPropertyName("n")]
        public long NumberOfTrades { get; set; }

        [JsonPropertyName("vw")]
        public decimal VolumeWeightedAveragePrice { get; set; }
    }
}
