
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace UnifyTrade.Exchange.Huobi;

internal class HuobiTradeReceiver : BackgroundService
{
    private readonly ChannelWriter<HuobiTradeRawData> _tradeWrite;
    public HuobiTradeReceiver(Channel<HuobiTradeRawData> tradeWrite)
    {
        _tradeWrite = tradeWrite.Writer;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var ws = new ClientWebSocket())
        {
            await ws.ConnectAsync(new Uri("wss://api.huobi.pro/ws"), stoppingToken);
            byte[] buffer = new byte[1024];
            string subscribe = @"{""sub"": ""market.btcusdt.trade.detail"",""id"": ""1000""}";
            byte[] conSubs = Encoding.UTF8.GetBytes(subscribe);
            await ws.SendAsync(conSubs, WebSocketMessageType.Text, true, stoppingToken);

            while (true)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, stoppingToken);
                }
                else
                {
                    string buffer2 = Decompress(buffer);
                    if (buffer2.Contains("ping"))
                    {
                        var x = JsonSerializer.Deserialize<HuobiPingData>(buffer2);
                        byte[] pong = Encoding.UTF8.GetBytes(@"{""pong"":" + x.Ping + " }");
                        await ws.SendAsync(pong, WebSocketMessageType.Text, true, stoppingToken);
                    }
                    else if (buffer2.Contains("subbed"))
                    {
                        continue;
                    }
                    else
                    {
                        await _tradeWrite.WriteAsync(new HuobiTradeRawData { RawData = Encoding.UTF8.GetBytes(buffer2) }, stoppingToken);
                    }
                }
            }
        }
    }
    static string Decompress(byte[] bytes)
    {
        using var memoryStream = new MemoryStream(bytes);
        using var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        using var memoryStreamOutput = new MemoryStream();

        gZipStream.CopyTo(memoryStreamOutput);
        var outputBytes = memoryStreamOutput.ToArray();

        string decompressed = Encoding.UTF8.GetString(outputBytes);
        return decompressed;
    }
}

public class HuobiPingData
{
    [JsonPropertyName("ping")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double Ping { get; set; }
}
