using System.Text.Json;
using System.Text.Json.Serialization;
using CsvHelper.Configuration;

public class Ticker
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
public sealed class TickerCsvMap : ClassMap<Ticker>
{
    public TickerCsvMap()
    {
        Map(m => m.Symbol).Name("ticker");
        Map(m => m.Name).Name("company name");
    }
}