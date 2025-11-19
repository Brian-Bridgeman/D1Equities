using System.Text.Json.Serialization;

public class Trade
{
    [JsonPropertyName("T")]
    public string MessageType { get; init; } = "";

    [JsonPropertyName("S")]
    public string Symbol { get; init; } = "";

    [JsonPropertyName("i")]
    public long TradeId { get; init; }

    [JsonPropertyName("x")]
    public string Exchange { get; init; } = "";

    [JsonPropertyName("p")]
    public decimal Price { get; init; }

    [JsonPropertyName("s")]
    public long Size { get; init; }

    [JsonPropertyName("c")]
    public List<string> Conditions { get; init; } = [];

    [JsonPropertyName("z")]
    public string Tape { get; init; } = "";

    [JsonPropertyName("t")]
    public DateTime Timestamp { get; init; }
}
