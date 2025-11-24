using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Websocket.Client;

namespace D1Equities.Sim
{
    public class MarketSimulator : IDisposable
    {
        private readonly HttpClient _httpClient;

        private readonly string _apiKeyId = Environment.GetEnvironmentVariable("APCA_API_KEY_ID");

        private readonly string _apiKeySecret = Environment.GetEnvironmentVariable(
            "APCA_API_SECRET_KEY"
        );

        private readonly int _candleLimit = 1000;

        public string ChartTimeFrame { get; set; } = "1Min";

        private WebsocketClient _webSocketClient { get; set; }

        public Dictionary<string, Ticker> AvailableSymbols { get; private set; } = [];

        public Stock? SelectedStock { get; set; }

        private Dictionary<string, Stock> _loadedStocks = [];

        public MarketMoversResponse? MarketMovers { get; private set; }
        public MostActiveStockResponse? MostActiveStocks { get; private set; }

        public Stock GetLoadedStock(string symbol) => _loadedStocks[symbol];
        
        public string[] GetAllLoadedSymbols() => _loadedStocks.Keys.ToArray();

        public MarketSimulator()
        {
            _httpClient = new();
        }

        public async Task InitAsync()
        {
            // Run tasks in parallel for maximum speed
            var wsTask = InitWebsocketClient();
            var activeTask = GetMostActiveStocks();
            var moversTask = GetMarketMovers();
            var symbolsTask = LoadAllUsSymbolsAsync();

            await Task.WhenAll(wsTask, activeTask, moversTask, symbolsTask);

            _webSocketClient = wsTask.Result;
            MostActiveStocks = activeTask.Result;
            MarketMovers = moversTask.Result;

        }

        public async Task LoadAllUsSymbolsAsync()
        {
            using var client = new HttpClient();

            var nasdaqTask = client.GetStringAsync("https://www.nasdaqtrader.com/dynamic/symdir/nasdaqlisted.txt");
            var otherTask = client.GetStringAsync("https://www.nasdaqtrader.com/dynamic/symdir/otherlisted.txt");

            await Task.WhenAll(nasdaqTask, otherTask);

            string nasdaqText = nasdaqTask.Result;
            string otherText = otherTask.Result;

            var availableSymbols = new List<Ticker>();

            ParseSymbolFile(nasdaqText);
            ParseSymbolFile(otherText);
        }

        private void ParseSymbolFile(string text)
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            //Hoppa över header och footer
            foreach (var line in lines.Skip(1).SkipLast(1))
            {
                var parts = line.Split('|');

                if (parts.Length >= 2)
                {
                    string symbol = parts[0].Trim();
                    string name = parts[1].Trim();

                    AvailableSymbols.TryAdd(symbol, new Ticker
                    {
                        Symbol = symbol,
                        Name = name
                    });
                }
            }
        }

        private string TrimCompanyName(string name)
        {
            name = name.Trim();

            return name.Length <= 50
                ? name
                : name.Substring(0, 50).TrimEnd() + "...";
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

        public async Task LoadStock(string symbol)
        {
            var stock = new Stock(symbol) { PriceHistory = await GetStockHistoryAsync(symbol) };
            if (stock.PriceHistory.Count == 0)
                return;

            stock.CurrentCandle = stock.PriceHistory.Last();

            //Alpaca allows max 30 websocket subscriptions
            if(_loadedStocks.Count <= 30)
                await SubscribeStockAsync(stock.Symbol);

            if (!_loadedStocks.TryAdd(stock.Symbol, stock))
                throw new Exception($"Couldnt add '{symbol}' to loaded stocks");
        }

        public async Task UnloadStock(string symbol)
        {
            if (!_loadedStocks.ContainsKey(symbol))
                throw new Exception($"Cannot remove stock with '{symbol}' because its not loaded");

            UnsubscribeStock(symbol);
            _loadedStocks.Remove(symbol);
        }

        public async Task UnloadAllStocks()
        {
            foreach(var key in _loadedStocks.Keys)
            {
                UnloadStock(key);
            }
        }

        public bool IsStockLoaded(string symbol) => _loadedStocks.ContainsKey(symbol);

        private async Task<List<CandleStick>> GetStockHistoryAsync(string symbol)
        {
            var dates = Enumerable
                .Range(0, 5)   
                .Select(i => DateTime.UtcNow.AddDays(-i).AddMinutes(-15).AddSeconds(-15)) // Free api key = 15 minute delayed data AND ACTUALLY 15 SECONDS < ALPACA FYCJKHG ljugare bre
                .Select(d => d.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                .ToArray();

            for(int i = 1; i < dates.Length; i++)
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(
                        $"https://data.alpaca.markets/v2/stocks/bars?symbols={symbol}&timeframe={ChartTimeFrame}&start={dates[i]}&end={dates[i-1]}&limit={_candleLimit}&adjustment=raw&feed=sip&sort=asc"
                    ),
                    Headers =
                    {
                        { "accept", "application/json" },
                        { "APCA-API-KEY-ID", _apiKeyId },
                        { "APCA-API-SECRET-KEY", _apiKeySecret },
                    },
                };
                using var response = await _httpClient.SendAsync(request);
                try 
                { 
                    
                    response.EnsureSuccessStatusCode();
                }
                catch
                {
                    if (i == dates.Length - 1)
                        throw new HttpRequestException("Couldnt get historical data");

                    continue;
                }

                var historicalBarsReponse =
                    await response.Content.ReadFromJsonAsync<HistoricalBarsResponse>();

                if (historicalBarsReponse != null && historicalBarsReponse.Bars.Count == 0)
                    continue;

                return historicalBarsReponse?.Bars[symbol] ?? [];
            }

            return [];
        }
        
        private async Task<MostActiveStockResponse?> GetMostActiveStocks()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://data.alpaca.markets/v1beta1/screener/stocks/most-actives?by=volume&top=10"),
                Headers =
                    {
                        { "accept", "application/json" },
                        { "APCA-API-KEY-ID", _apiKeyId },
                        { "APCA-API-SECRET-KEY", _apiKeySecret },
                    },
            };

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<MostActiveStockResponse>();
            }
        }

        private async Task<MarketMoversResponse?> GetMarketMovers()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://data.alpaca.markets/v1beta1/screener/stocks/movers?top=10"),
                Headers =
                    {
                        { "accept", "application/json" },
                        { "APCA-API-KEY-ID", _apiKeyId },
                        { "APCA-API-SECRET-KEY", _apiKeySecret },
                    },
            };
            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<MarketMoversResponse>();
            }
        }

        private void UnsubscribeStock(string symbol)
        {
            var unsubscribeMsg = JsonSerializer.Serialize(
                new
                {
                    action = "unsubscribe",
                    trades = new[] { symbol },
                    bars = new[] { symbol },
                }
            );

            _webSocketClient.Send(unsubscribeMsg);
        }

        private async Task SubscribeStockAsync(string symbol) 
        {
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
                        var trade = JsonSerializer.Deserialize<Trade>(item);
                        HandleTradeMessage(trade);
                        break;
                    case "b":
                        var candleStick = JsonSerializer.Deserialize<CandleStick>(item);
                        HandleBarMessage(candleStick);
                        break;
                }
            }
        }

        private void HandleBarMessage(CandleStick? candleStick)
        {
            if (!_loadedStocks.TryGetValue(candleStick.Symbol, out var stock))
                throw new Exception("Received websocket on symbol that is not in loaded stocks");

            stock.PriceHistory.Add(stock.CurrentCandle);
            stock.CurrentCandle = candleStick;
            stock.OnNewCandle(candleStick);
        }

        private void HandleTradeMessage(Trade? trade)
        {
            if (!_loadedStocks.TryGetValue(trade.Symbol, out var stock))
                throw new Exception("Received websocket on symbol that is not in loaded stocks");

            var candle = stock.CurrentCandle;

            candle.High = Math.Max(candle.High, trade.Price);
            candle.Low = Math.Min(candle.Low, trade.Price);
            candle.Close = trade.Price;

            stock.OnCandleUpdated(candle);
        }

        public void Dispose()
        {
            _webSocketClient.Dispose();
            _httpClient.Dispose();
        }
    }
}
