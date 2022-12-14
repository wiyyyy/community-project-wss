using Microsoft.AspNetCore.SignalR;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

internal class BackgroundTradeSender : BackgroundService
{
    private readonly ChannelReader<byte[]> _tradeReader;
    private readonly IHubContext<TradeHub> _hubContext;
    public BackgroundTradeSender(Channel<byte[]> tradeReader, IHubContext<TradeHub> hubContext)
    {
        _tradeReader = tradeReader.Reader;
        _hubContext = hubContext;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var trade = await _tradeReader.ReadAsync(stoppingToken);
            var serializedTrade = JsonSerializer.Deserialize<BinanceStreamTrade>(trade);
            //Console.WriteLine("{0} - {1} - {2} - {3}", serializedTrade.Symbol, serializedTrade.Quantity, serializedTrade.Price, serializedTrade.BuyerIsMaker ? "SELL" : "BUY");
            //Console.WriteLine(Encoding.UTF8.GetString(trade));
            if(serializedTrade.Quantity * serializedTrade.Price > 100)
                await _hubContext.Clients.All.SendAsync("aggrTrade", serializedTrade, cancellationToken: stoppingToken);
        }
    }
}


public class BinanceStreamTrade
{
    /// <summary>
    /// The symbol the trade was for
    /// </summary>
    [JsonPropertyName("s")]
    [JsonIgnore]
    public string Symbol { get; set; } = string.Empty;
    /// <summary>
    /// The id of this trade
    /// </summary>
    [JsonPropertyName("t")]
    [JsonIgnore]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long Id { get; set; }
    /// <summary>
    /// The price of the trades
    /// </summary>
    [JsonPropertyName("p")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Price { get; set; }
    /// <summary>
    /// The quantity of the trade
    /// </summary>
    [JsonPropertyName("q")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Quantity { get; set; }
    /// <summary>
    /// The buyer order id
    /// </summary>
    [JsonPropertyName("b")]
    [JsonIgnore]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long BuyerOrderId { get; set; }
    /// <summary>
    /// The sell order id
    /// </summary>
    [JsonPropertyName("a")]
    [JsonIgnore]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long SellerOrderId { get; set; }
    /// <summary>
    /// The time of the trade
    /// </summary>
    [JsonPropertyName("T")]
    [JsonIgnore]
    public DateTime TradeTime { get; set; }
    /// <summary>
    /// Whether the buyer was the maker
    /// </summary>
    [JsonPropertyName("m")]
    public bool BuyerIsMaker { get; set; }

    /// <summary>
    /// Unused
    /// </summary>
    [JsonPropertyName("M")]
    [JsonIgnore]
    public bool IsBestMatch { get; set; }
}