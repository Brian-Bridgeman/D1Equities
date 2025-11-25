using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;
using Websocket.Client;

namespace D1Equities.Sim
{
    public class MarketSimulator : IDisposable
    {
        public enum WebSocketErrorCode
        {
            InvalidSyntax = 400,
            NotAuthenticated = 401,
            AuthFailed = 402,
            AlreadyAuthenticated = 403,
            AuthTimeout = 404,
            SymbolLimitExceeded = 405,
            ConnectionLimitExceeded = 406,
            SlowClient = 407,
            InsufficientSubscription = 409,
            InvalidSubscribeAction = 410,
            InternalError = 500
        }

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
            _webSocketClient = await InitWebsocketClient();

            var activeTask = GetMostActiveStocks();
            var moversTask = GetMarketMovers();
            var symbolsTask = LoadAllUsSymbolsAsync();

            await Task.WhenAll(activeTask, moversTask, symbolsTask);

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

        private async Task<WebsocketClient> InitWebsocketClient()
        {
            var client = new WebsocketClient(new Uri("wss://stream.data.alpaca.markets/v2/iex"))
            {
                ReconnectTimeout = TimeSpan.FromSeconds(10),
            };

            await client.Start();
            //client.DisconnectionHappened.Subscribe(info =>
            //{
            //    throw new Exception($"Websocket disconnected: {info.Type} - {info.CloseStatusDescription}");
            //});

            AuthenticateWebsocket(client);

            client.MessageReceived.Subscribe(msg =>
            {
                if (!string.IsNullOrEmpty(msg.Text))
                    HandleWebSocketMessage(msg.Text);
            });

            return client;
        }
        
        private void AuthenticateWebsocket(WebsocketClient client)
        {
            var authMessage = JsonSerializer.Serialize(
                new
                {
                    action = "auth",
                    key = _apiKeyId,
                    secret = _apiKeySecret,
                }
            );

            client.Send(authMessage);
        }

        public async Task LoadStocks(string[] symbols)
        {
            foreach (var symbol in symbols)
            {
                var stock = new Stock(symbol) { PriceHistory = await GetStockHistoryAsync(symbol) };

                if (stock.PriceHistory.Count == 0)
                    return;

                stock.CurrentCandle = stock.PriceHistory.Last();

                //Alpaca allows max 30 websocket subscriptions
                if (_loadedStocks.Count >= 30)
                    throw new Exception("Cannot load more than 30 stocks");

                if (!_loadedStocks.TryAdd(stock.Symbol, stock))
                    throw new Exception($"Couldnt add '{symbol}' to loaded stocks");
            }

            SubscribeStocks(symbols);
        }
        public void UnsubscribeAllStocks(string[] excludedSymbols)
        {
            var symbols = _loadedStocks.Keys.Where(s => !excludedSymbols.Contains(s)).ToArray();
            if (symbols.Length == 0)
                return;

            foreach(var symbol in symbols)
                _loadedStocks.Remove(symbol);

            var unsubscribeMsg = JsonSerializer.Serialize(
                new
                {
                    action = "unsubscribe",
                    trades = symbols,
                    bars = symbols,
                }
            );

            _webSocketClient.Send(unsubscribeMsg);
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

        public void SubscribeStocks(string[] symbols) 
        {
            var subscribeMsg = JsonSerializer.Serialize(
                new
                {
                    action = "subscribe",
                    trades = symbols,
                    bars = symbols,
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
                var type = item.GetProperty("T").GetString();

                switch (type)
                {
                    case "t":
                        var trade = JsonSerializer.Deserialize<Trade>(item);
                        HandleTradeMessage(trade);
                        break;

                    case "b":
                        var candle = JsonSerializer.Deserialize<CandleStick>(item);
                        HandleBarMessage(candle);
                        break;

                    case "error":
                        var msg = item.GetProperty("msg").GetString();
                        var code = item.GetProperty("code").GetInt32();

                        HandleWebSocketError(code, msg);
                        break;
                }
            }
        }

        private void HandleWebSocketError(int code, string msg)
        {
            switch ((WebSocketErrorCode)code)
            {
                case WebSocketErrorCode.InvalidSyntax:
                    throw new Exception(
                        $"{code} {msg} - The message you sent to the server did not follow the specification. This can also be sent if the symbol in your subscription message is in invalid format."
                    );

                case WebSocketErrorCode.NotAuthenticated:
                    AuthenticateWebsocket(_webSocketClient);
                    break;

                case WebSocketErrorCode.AuthFailed:
                    throw new Exception(
                        $"{code} {msg} - You have provided invalid authentication credentials."
                    );

                case WebSocketErrorCode.AlreadyAuthenticated:
                    return;

                case WebSocketErrorCode.AuthTimeout:
                    throw new Exception(
                        $"{code} {msg} - You failed to successfully authenticate after connecting. You only have a few seconds to authenticate after connecting."
                    );

                case WebSocketErrorCode.SymbolLimitExceeded:
                    throw new Exception(
                        $"{code} {msg} - The symbol subscription request you sent would put you over the limit set by your subscription package. If this happens your symbol subscriptions are the same as they were before you sent the request that failed."
                    );

                case WebSocketErrorCode.ConnectionLimitExceeded:
                    throw new Exception(
                        $"{code} {msg} - You already have the number of sessions allowed by your subscription."
                    );

                case WebSocketErrorCode.SlowClient:
                    throw new Exception(
                        $"{code} {msg} - You may receive this if you are too slow to process the messages sent by the server. This is not guaranteed to arrive before you are disconnected to avoid keeping slow connections active forever."
                    );

                case WebSocketErrorCode.InsufficientSubscription:
                    throw new Exception(
                        $"{code} {msg} - You have attempted to access a data source not available in your subscription package."
                    );

                case WebSocketErrorCode.InvalidSubscribeAction:
                    throw new Exception(
                        $"{code} {msg} - You tried to subscribe to channels not available in the stream, for example to bars in the option stream or to trades in the news stream."
                    );

                case WebSocketErrorCode.InternalError:
                    throw new Exception(
                        $"{code} {msg} - An unexpected error occurred on our end. Please let us know if this happens."
                    );

                default:
                    throw new Exception($"{code} {msg} - Unknown websocket error.");
            }
        }


        private void HandleBarMessage(CandleStick? candleStick)
        {
            if (!_loadedStocks.TryGetValue(candleStick.Symbol, out var stock))
                return;
                //throw new Exception("Received websocket message on stock that isnt loaded");

            stock.PriceHistory.Add(stock.CurrentCandle);
            stock.CurrentCandle = candleStick;
            stock.OnNewCandle(candleStick);
        }

        private void HandleTradeMessage(Trade? trade)
        {
            if (!_loadedStocks.TryGetValue(trade.Symbol, out var stock))
                return;
                //throw new Exception("Received websocket message on stock that isnt loaded");

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
