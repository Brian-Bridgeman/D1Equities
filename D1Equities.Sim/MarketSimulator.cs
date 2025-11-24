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

        public string[] AvailableSymbols { get; }

        public Stock? SelectedStock { get; set; }

        private Dictionary<string, Stock> _loadedStocks = [];

        public Stock GetLoadedStock(string symbol) => _loadedStocks[symbol];
        
        public string[] GetAllLoadedSymbols() => _loadedStocks.Keys.ToArray();

        public MarketSimulator()
        {
            _httpClient = new();
            AvailableSymbols = [
                "A", "AAL", "AAP", "AAPL", "ABBV", "ABC", "ABNB", "ABT", "ACGL", "ACH",
                "ACN", "ADBE", "ADI", "ADM", "ADP", "ADSK", "AEE", "AEP", "AES", "AFL",
                "AIG", "AIZ", "AJG", "AKAM", "ALB", "ALGN", "ALK", "ALL", "ALLE", "AMAT",
                "AMCR", "AMD", "AME", "AMGN", "AMP", "AMT", "AMZN", "ANET", "ANR", "AON",
                "AOS", "APA", "APD", "APH", "APO", "APP", "APTV", "ARE", "ATO", "AVB",
                "AVGO", "AVY", "AWK", "AXON", "AXP", "AZO", "BA", "BAC", "BALL", "BAX",
                "BBWI", "BBY", "BDX", "BEN", "BF.B", "BG", "BIIB", "BK", "BKNG", "BKR",
                "BLDR", "BLK", "BMY", "BR", "BRK.B", "BRO", "BSX", "BSY", "BUD", "BX",
                "BXP", "C", "CAG", "CAH", "CARR", "CAT", "CB", "CBOE", "CBRE", "CCI",
                "CCL", "CDAY", "CDNS", "CDW", "CE", "CEG", "CF", "CFG", "CHD", "CHRW",
                "CHTR", "CI", "CINF", "CL", "CLX", "CMCSA", "CME", "CMG", "CMI", "CMS",
                "CNC", "CNP", "COF", "COIN", "COO", "COP", "COR", "COST", "CPB", "CPG",
                "CPRT", "CPT", "CRL", "CRM", "CRWD", "CSCO", "CSGP", "CSX", "CTAS", "CTRA",
                "CTSH", "CTVA", "CVS", "CVX", "D", "DAL", "DASH", "DAY", "DD", "DDOG",
                "DE", "DECK", "DELL", "DFS", "DG", "DGX", "DHI", "DHR", "DIS", "DLR",
                "DLTR", "DOC", "DOV", "DOW", "DPZ", "DRI", "DTE", "DUK", "DVA", "DVN",
                "DXCM", "EA", "EBAY", "ECL", "ED", "EFX", "EG", "EIX", "EL", "ELV",
                "EME", "EMR", "EOG", "EPAM", "EQIX", "EQR", "EQT", "ERIE", "ES", "ESS",
                "ETN", "ETR", "EVRG", "EW", "EXC", "EXE", "EXPD", "EXPE", "EXR", "F",
                "FANG", "FAST", "FCX", "FDS", "FDX", "FE", "FFIV", "FICO", "FIS", "FISV",
                "FITB", "FL", "FLS", "FLT", "FMC", "FOX", "FOXA", "FRT", "FSLR", "FTNT",
                "FTV", "GD", "GDDY", "GE", "GEHC", "GEN", "GEV", "GILD", "GIS", "GL",
                "GLW", "GM", "GNRC", "GOOG", "GOOGL", "GPC", "GPN", "GRMN", "GS", "GWW",
                "HAL", "HAS", "HBAN", "HCA", "HD", "HES", "HIG", "HII", "HLT", "HOLX",
                "HON", "HOOD", "HPE", "HPQ", "HRL", "HSIC", "HST", "HSY", "HUBB", "HUM",
                "HWM", "IBM", "ICE", "IDXX", "IEX", "IFF", "ILMN", "INCY", "INTC", "INTU",
                "INVH", "IP", "IPG", "IQV", "IR", "IRM", "ISRG", "IT", "ITW", "IVZ",
                "J", "JBHT", "JBL", "JCI", "JKHY", "JNJ", "JPM", "K", "KDP", "KEY",
                "KEYS", "KHC", "KIM", "KKR", "KLAC", "KMB", "KMI", "KO", "KR", "KVUE",
                "L", "LDOS", "LEN", "LH", "LHX", "LII", "LIN", "LKQ", "LLY", "LMT",
                "LNT", "LOW", "LRCX", "LULU", "LUV", "LVS", "LW", "LYB", "LYV", "MA",
                "MAA", "MAC", "MAR", "MAS", "MCD", "MCHP", "MCK", "MCO", "MDLZ", "MDT",
                "MET", "META", "MGM", "MHK", "MKC", "MLM", "MMC", "MMM", "MNST", "MO",
                "MOH", "MOS", "MPC", "MPWR", "MRK", "MRNA", "MS", "MSCI", "MSFT", "MSI",
                "MTB", "MTCH", "MTD", "MU", "NCLH", "NDAQ", "NDSN", "NEE", "NEM", "NFLX",
                "NI", "NKE", "NOC", "NOW", "NRG", "NSC", "NTAP", "NTRS", "NUE", "NVDA",
                "NVR", "NWS", "NWSA", "NXPI", "O", "ODFL", "OKE", "OMC", "ON", "ORCL",
                "ORLY", "OTIS", "OXY", "PANW", "PAYC", "PAYX", "PCAR", "PCG", "PEG", "PEP",
                "PFE", "PFG", "PG", "PGR", "PH", "PHM", "PKG", "PLD", "PLTR", "PM",
                "PNC", "PNR", "PNW", "PODD", "POOL", "PPG", "PPL", "PRU", "PSA", "PSKY",
                "PSX", "PTC", "PWR", "PYPL", "Q", "QCOM", "RCL", "REG", "REGN", "RF",
                "RJF", "RL", "RMD", "ROK", "ROL", "ROP", "ROST", "RSG", "RTX", "RVTY",
                "SBAC", "SBUX", "SCHW", "SHW", "SJM", "SLB", "SMCI", "SNA", "SNPS", "SO",
                "SOLS", "SOLV", "SPG", "SPGI", "SRE", "STE", "STLD", "STT", "STX", "STZ",
                "SW", "SWK", "SWKS", "SYF", "SYK", "SYY", "T", "TAP", "TDG", "TDY",
                "TECH", "TEL", "TER", "TFC", "TGT", "TJX", "TKO", "TMO", "TMUS", "TPL",
                "TPR", "TRGP", "TRMB", "TROW", "TRV", "TSCO", "TSLA", "TSN", "TT", "TTD",
                "TTWO", "TXN", "TXT", "TYL", "UAL", "UBER", "UDR", "UHS", "ULTA", "UNH",
                "UNP", "UPS", "URI", "USB", "V", "VICI", "VLO", "VLTO", "VMC", "VRSK",
                "VRSN", "VRTX", "VST", "VTR", "VTRS", "VZ", "WAB", "WAT", "WBD", "WDC",
                "WEC", "WELL", "WFC", "WM", "WMB", "WMT", "WRB", "WSM", "WST", "WTW",
                "WY", "WYNN", "XEL", "XOM", "XYL", "XYZ", "YUM", "ZBH", "ZBRA", "ZTS"
                ];
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

        public async Task LoadStock(string symbol)
        {
            var stock = new Stock(symbol) { PriceHistory = await GetStockHistoryAsync(symbol) };
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
                .Select(i => DateTime.UtcNow.AddDays(-i).AddMinutes(-15).AddSeconds(-15)) // Free api key = 15 minute delayed data AND ACTUALLY 15 SECONDS < ALPACA FYCJKHG LIAAARS
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
