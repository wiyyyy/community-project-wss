using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;

namespace UnifyTrade.Exchange.Bybit;

internal class BybitTradeReceiver : BackgroundService
{
    private readonly ChannelWriter<BybitTradeRawData> _tradeWrite;
    public BybitTradeReceiver(Channel<BybitTradeRawData> tradeWrite)
    {
        _tradeWrite = tradeWrite.Writer;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var ws = new ClientWebSocket())
        {
            await ws.ConnectAsync(new Uri("wss://stream.bybit.com/realtime_public"), stoppingToken);

            Memory<byte> buffer = new byte[1024 * 32];

            string subscribe = "{\"op\":\"subscribe\",\"args\":[\"trade.BTCUSDT\"]}";

            byte[] conSubs = Encoding.UTF8.GetBytes(subscribe);

            while (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(conSubs, WebSocketMessageType.Text, true, stoppingToken);
                var result = await ws.ReceiveAsync(buffer, stoppingToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, stoppingToken);
                }
                else
                {
                    var stringRawBuffer = Encoding.UTF8.GetString(buffer.ToArray());

                    if (stringRawBuffer.Contains("\"success\"")) continue;

                    await _tradeWrite.WriteAsync(new BybitTradeRawData { RawData = buffer[..result.Count].ToArray() }, stoppingToken);
                }
            }
        }
    }
}
