using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace UnifyTrade.Exchange.Bybit;

internal class BybitTradeSender : BackgroundService
{
    private readonly ChannelReader<BybitTradeRawData> _tradeReader;
    private readonly IHubContext<TradeHub> _hubContext;
    public BybitTradeSender(Channel<BybitTradeRawData> tradeReader, IHubContext<TradeHub> hubContext)
    {
        _tradeReader = tradeReader.Reader;
        _hubContext = hubContext;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var trade = await _tradeReader.ReadAsync(stoppingToken);
            var serializedTrade = JsonSerializer.Deserialize<BybitStreamTrade>(trade.RawData);

            for (int i = 0; i < serializedTrade.Data.Length; i++)
            {
                await _hubContext.Clients.All.SendAsync("aggrTrade", new TradeData
                {
                    Source = "Bybit",
                    Direction = serializedTrade.Data[i].Side != "Buy",
                    Price = serializedTrade.Data[i].Price,
                    Quantity = serializedTrade.Data[i].Size
                }, cancellationToken: stoppingToken);
            }
        }
    }
}
public class BybitStreamTrade
{
    [JsonPropertyName("topic")]
    public string Topic { get; set; }

    [JsonPropertyName("data")]
    public BybitStreamTradeData[] Data { get; set; }
}

public class BybitStreamTradeData
{
    [JsonPropertyName("symbol")]
    [JsonIgnore]
    public string Symbol { get; set; }

    [JsonPropertyName("tick_direction")]
    [JsonIgnore]
    public string TickDirection { get; set; }

    [JsonPropertyName("price")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Price { get; set; }

    [JsonPropertyName("size")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Size { get; set; }

    [JsonPropertyName("timestamp")]
    [JsonIgnore]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("trade_time_ms")]
    [JsonIgnore]
    public string TradeTimeMs { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; }

    [JsonPropertyName("trade_id")]
    [JsonIgnore]
    public Guid TradeId { get; set; }

    [JsonPropertyName("is_block_trade")]
    [JsonIgnore]
    public bool IsBlockTrade { get; set; }
}