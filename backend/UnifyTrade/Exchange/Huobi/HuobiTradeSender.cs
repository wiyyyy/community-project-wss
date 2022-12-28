using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace UnifyTrade.Exchange.Huobi;
internal class HuobiTradeSender : BackgroundService
{
    private readonly ChannelReader<HuobiTradeRawData> _tradeReader;
    private readonly IHubContext<TradeHub> _hubContext;
    public HuobiTradeSender(Channel<HuobiTradeRawData> tradeReader, IHubContext<TradeHub> hubContext)
    {
        _tradeReader = tradeReader.Reader;
        _hubContext = hubContext;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var trade = await _tradeReader.ReadAsync(stoppingToken);
            var serializedTrade = JsonSerializer.Deserialize<HuobiTradeStream>(trade.RawData);

            for (int i = 0; i < serializedTrade.tick.data.Length; i++)
            {
                await _hubContext.Clients.All.SendAsync("aggrTrade", new TradeData
                {
                    Source = "Huobi",
                    Direction = serializedTrade.tick.data[i].direction != "buy",
                    Price = serializedTrade.tick.data[i].price,
                    Quantity = serializedTrade.tick.data[i].amount
                }, cancellationToken: stoppingToken);
            }
        }
    }
}

public class HuobiTradeStream
{
    public string ch { get; set; }
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double ts { get; set; }
    public Tick tick { get; set; }
}

public class Tick
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double id { get; set; }
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double ts { get; set; }
    public Datum[] data { get; set; }
}

public class Datum
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double id { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double ts { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double tradeId { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal amount { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal price { get; set; }

    public string direction { get; set; }
}
