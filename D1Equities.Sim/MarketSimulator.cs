using System.Net.Http.Json;
using System.Text.Json;
using Websocket.Client;

namespace D1Equities.Sim
{
    public class MarketSimulator
    {
        private readonly HttpClient _httpClient;

        private readonly string _apiKeyId = Environment.GetEnvironmentVariable("APCA_API_KEY_ID");

        private readonly string _apiKeySecret = Environment.GetEnvironmentVariable(
            "APCA_API_SECRET_KEY"
        );

        private readonly int _candleLimit = 1000;

        public string ChartTimeFrame { get; set; } = "1Min";

        private WebsocketClient _webSocketClient { get; set; }

        public string[] AvailableSymbols { get; }

        public Stock? SelectedStock { get; set; }

        public MarketSimulator()
        {
            _httpClient = new();
            //TODO läs in csv med alla symbols från S&P 500
            AvailableSymbols = ["AAPL", "NVDA", "TSLA", "MSFT", "AMZN"];
        }

        public async Task InitAsync()
        {
            _webSocketClient = await InitWebsocketClient();
        }

        private async Task<WebsocketClient> InitWebsocketClient()
        {
            var client = new WebsocketClient(new Uri("wss://stream.data.alpaca.markets/v2/iex"))
            {
                ReconnectTimeout = TimeSpan.FromSeconds(10),
            };

            await client.Start();

            var authMessage = JsonSerializer.Serialize(
                new
                {
                    action = "auth",
                    key = _apiKeyId,
                    secret = _apiKeySecret,
                }
            );

            client.Send(authMessage);

            client.MessageReceived.Subscribe(msg =>
            {
                if (!string.IsNullOrEmpty(msg.Text))
                    HandleWebSocketMessage(msg.Text);
            });

            return client;
        }

        public async Task SelectStock(string symbol)
        {
            //UnsubscribeCurrentStock();
            SelectedStock = new Stock(symbol) { PriceHistory = await GetStockHistoryAsync(symbol) };
            //await SubscribeStockAsync();
        }

        private async Task<List<CandleStick>> GetStockHistoryAsync(string symbol)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    $"https://data.alpaca.markets/v2/stocks/bars?symbols={symbol}&timeframe={ChartTimeFrame}&start=2025-11-18T00%3A00%3A00Z&end=2025-11-19T00%3A00%3A00Z&limit={_candleLimit}&adjustment=raw&feed=sip&sort=asc"
                ),
                Headers =
                {
                    { "accept", "application/json" },
                    { "APCA-API-KEY-ID", _apiKeyId },
                    { "APCA-API-SECRET-KEY", _apiKeySecret },
                },
            };
            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var historicalBarsReponse =
                await response.Content.ReadFromJsonAsync<HistoricalBarsResponse>();

            return historicalBarsReponse?.Bars[symbol] ?? [];
        }

        private void UnsubscribeCurrentStock()
        {
            if (SelectedStock == null)
                return;

            var unsubscribeMsg = JsonSerializer.Serialize(
                new
                {
                    action = "unsubscribe",
                    trades = new[] { SelectedStock.Symbol },
                    bars = new[] { SelectedStock.Symbol },
                }
            );

            _webSocketClient.Send(unsubscribeMsg);
        }

        private async Task SubscribeStockAsync() 
        {
            if (SelectedStock == null)
                throw new InvalidOperationException("No stock selected.");

            var symbol = SelectedStock.Symbol;

            var subscribeMsg = JsonSerializer.Serialize(
                new
                {
                    action = "subscribe",
                    trades = new[] { symbol },
                    bars = new[] { symbol },
                }
            );

            _webSocketClient.Send(subscribeMsg);
        }

        private void HandleWebSocketMessage(string json)
        {
            var messages = JsonSerializer.Deserialize<List<JsonElement>>(json);
            if (messages == null)
                return;

            foreach (var item in messages)
            {
                switch (item.GetProperty("T").GetString())
                {
                    case "t":
                        var trade = JsonSerializer.Deserialize<Trade>(json);
                        HandleTradeMessage(trade);
                        break;

                    case "b":
                        var candleStick = JsonSerializer.Deserialize<CandleStick>(json);
                        HandleBarMessage(candleStick);
                        break;
                }
            }
        }

        private void HandleBarMessage(CandleStick? candleStick)
        {
            if (candleStick == null)
                return;

            SelectedStock.PriceHistory.Add(SelectedStock.CurrentCandle);
            SelectedStock.CurrentCandle = candleStick;

            SelectedStock.OnCandleUpdated(candleStick);
        }

        private void HandleTradeMessage(Trade? trade)
        {
            if (SelectedStock?.CurrentCandle == null)
                return;

            var candle = SelectedStock.CurrentCandle;

            candle.High = Math.Max(candle.High, trade.Price);
            candle.Low = Math.Min(candle.Low, trade.Price);
            candle.Close = trade.Price;

            SelectedStock.OnCandleUpdated(candle);
        }
    }
}
