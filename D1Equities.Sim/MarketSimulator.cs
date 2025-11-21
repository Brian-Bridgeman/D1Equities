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

        private Dictionary<string, Stock> _loadedStocks = [];

        public Stock? TryGetLoadedStock(string symbol) => _loadedStocks.TryGetValue(symbol, out var stock) ? stock : null;

        public MarketSimulator()
        {
            _httpClient = new();
            AvailableSymbols =
            [
                "MMM","AOS","ABT","ABBV","ACN","ADBE","AMD","AES","AFL","A",
                "APD","ABNB","AKAM","ALB","ARE","ALGN","ALLE","LNT","ALL","GOOGL",
                "GOOG","MO","AMZN","AMCR","AEE","AEP","AXP","AIG","AMT","AWK",
                "AMP","AME","AMGN","APH","ADI","AON","APA","APO","AAPL","AMAT",
                "APP","APTV","ACGL","ADM","ANET","AJG","AIZ","T","ATO","ADSK",
                "ADP","AZO","AVB","AVY","AXON","BKR","BALL","BAC","BAX","BDX",
                "BRK.B","BBY","TECH","BIIB","BLK","BX","XYZ","BK","BA","BKNG",
                "BSX","BMY","AVGO","BR","BRO","BF.B","BLDR","BG","BXP","CHRW",
                "CDNS","CPT","CPB","COF","CAH","CCL","CARR","CAT","CBOE","CBRE",
                "CDW","COR","CNC","CNP","CF","CRL","SCHW","CHTR","CVX","CMG",
                "CB","CHD","CI","CINF","CTAS","CSCO","C","CFG","CLX","CME",
                "CMS","KO","CTSH","COIN","CL","CMCSA","CAG","COP","ED","STZ",
                "CEG","COO","CPRT","GLW","CPAY","CTVA","CSGP","COST","CTRA","CRWD",
                "CCI","CSX","CMI","CVS","DHR","DRI","DDOG","DVA","DAY","DECK",
                "DE","DELL","DAL","DVN","DXCM","FANG","DLR","DG","DLTR","D",
                "DPZ","DASH","DOV","DOW","DHI","DTE","DUK","DD","ETN","EBAY",
                "ECL","EIX","EW","EA","ELV","EME","EMR","ETR","EOG","EPAM",
                "EQT","EFX","EQIX","EQR","ERIE","ESS","EL","EG","EVRG","ES",
                "EXC","EXE","EXPE","EXPD","EXR","XOM","FFIV","FDS","FICO","FAST",
                "FRT","FDX","FIS","FITB","FSLR","FE","FISV","F","FTNT","FTV",
                "FOXA","FOX","BEN","FCX","GRMN","IT","GE","GEHC","GEV","GEN",
                "GNRC","GD","GIS","GM","GPC","GILD","GPN","GL","GDDY","GS",
                "HAL","HIG","HAS","HCA","DOC","HSIC","HSY","HPE","HLT","HOLX",
                "HD","HON","HRL","HST","HWM","HPQ","HUBB","HUM","HBAN","HII",
                "IBM","IEX","IDXX","ITW","INCY","IR","PODD","INTC","IBKR","ICE",
                "IFF","IP","IPG","INTU","ISRG","IVZ","INVH","IQV","IRM","JBHT",
                "JBL","JKHY","J","JNJ","JCI","JPM","K","KVUE","KDP","KEY",
                "KEYS","KMB","KIM","KMI","KKR","KLAC","KHC","KR","LHX","LH",
                "LRCX","LW","LVS","LDOS","LEN","LII","LLY","LIN","LYV","LKQ",
                "LMT","L","LOW","LULU","LYB","MTB","MPC","MAR","MMC","MLM",
                "MAS","MA","MTCH","MKC","MCD","MCK","MDT","MRK","META","MET",
                "MTD","MGM","MCHP","MU","MSFT","MAA","MRNA","MHK","MOH","TAP",
                "MDLZ","MPWR","MNST","MCO","MS","MOS","MSI","MSCI","NDAQ","NTAP",
                "NFLX","NEM","NWSA","NWS","NEE","NKE","NI","NDSN","NSC","NTRS",
                "NOC","NCLH","NRG","NUE","NVDA","NVR","NXPI","ORLY","OXY","ODFL",
                "OMC","ON","OKE","ORCL","OTIS","PCAR","PKG","PLTR","PANW","PSKY",
                "PH","PAYX","PAYC","PYPL","PNR","PEP","PFE","PCG","PM","PSX",
                "PNW","PNC","POOL","PPG","PPL","PFG","PG","PGR","PLD","PRU",
                "PEG","PTC","PSA","PHM","PWR","QCOM","DGX","Q","RL","RJF",
                "RTX","O","REG","REGN","RF","RSG","RMD","RVTY","HOOD","ROK",
                "ROL","ROP","ROST","RCL","SPGI","CRM","SBAC","SLB","STX","SRE",
                "NOW","SHW","SPG","SWKS","SJM","SW","SNA","SOLS","SOLV","SO",
                "LUV","SWK","SBUX","STT","STLD","STE","SYK","SMCI","SYF","SNPS",
                "SYY","TMUS","TROW","TTWO","TPR","TRGP","TGT","TEL","TDY","TER",
                "TSLA","TXN","TPL","TXT","TMO","TJX","TKO","TTD","TSCO","TT",
                "TDG","TRV","TRMB","TFC","TYL","TSN","USB","UBER","UDR","ULTA",
                "UNP","UAL","UPS","URI","UNH","UHS","VLO","VTR","VLTO","VRSN",
                "VRSK","VZ","VRTX","VTRS","VICI","V","VST","VMC","WRB","GWW",
                "WAB","WMT","DIS","WBD","WM","WAT","WEC","WFC","WELL","WST",
                "WDC","WY","WSM","WMB","WTW","WDAY","WYNN","XEL","XYL","YUM",
                "ZBRA","ZBH","ZTS"
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

        public async Task SelectStock(string symbol)
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
            if (!_loadedStocks.TryGetValue(candleStick.Symbol, out var stock))
                throw new Exception("Received websocket on symbol that is not in loaded stocks");

            stock.PriceHistory.Add(stock.CurrentCandle);
            stock.CurrentCandle = candleStick;
            stock.OnCandleUpdated(candleStick);
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
        }
    }
}
