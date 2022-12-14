using System.Net.WebSockets;
using System.Threading.Channels;

internal class BackgroundTradeReceiver : BackgroundService
{
    private readonly ChannelWriter<byte[]> _tradeWrite;
    public BackgroundTradeReceiver(Channel<byte[]> tradeWrite)
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
                    await _tradeWrite.WriteAsync(buffer[..result.Count].ToArray(), stoppingToken);
                }
            }
        }
    }
}