using System.Net.WebSockets;
using System.Threading.Channels;

namespace UnifyTrade.Exchange.Binance;

internal class BinanceTradeReceiver : BackgroundService
{
    private readonly ChannelWriter<BinanceTradeRawData> _tradeWrite;
    public BinanceTradeReceiver(Channel<BinanceTradeRawData> tradeWrite)
    {
        _tradeWrite = tradeWrite.Writer;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var ws = new ClientWebSocket())
        {
            await ws.ConnectAsync(new Uri("wss://stream.binance.com:9443/ws/btcusdt@trade"), stoppingToken);
            Memory<byte> buffer = new byte[167];
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buffer, stoppingToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, stoppingToken);
                }
                else
                {
                    await _tradeWrite.WriteAsync(new BinanceTradeRawData { RawData = buffer[..result.Count].ToArray() }, stoppingToken);
                }
            }
        }
    }
}
