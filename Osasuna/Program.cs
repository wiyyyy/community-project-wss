// See https://aka.ms/new-console-template for more information
using BinanceWebSocket;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;

 


Send();


static void Send()
{

    CancellationTokenSource source = new CancellationTokenSource();
    using (var ws = new ClientWebSocket())
    {
        Console.WriteLine("Call starting..");
        ws.ConnectAsync(new Uri("wss://stream.bybit.com/realtime_public"), source.Token).Wait();
        byte[] buffer = new byte[1024 * 4];
        string subscribe = "{\"op\":\"subscribe\",\"args\":[\"trade.ETHUSDT\"]}";
        byte[] conSubs = Encoding.UTF8.GetBytes(subscribe);


        while (ws.State == WebSocketState.Open)
        {
            ws.SendAsync(conSubs, WebSocketMessageType.Text, true, source.Token).ConfigureAwait(false);
            var result = ws.ReceiveAsync(new ArraySegment<byte>(buffer), source.Token).Result;
            if (result.MessageType == WebSocketMessageType.Close)
            {
                ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, source.Token).Wait();
            }
            else
            {
                string responseMsg = Encoding.UTF8.GetString(buffer);
                //var lastResponseMsg = responseMsg.Replace("{\"topic\":\"trade.ETHUSDT\",:", String.Empty);
                Console.WriteLine(responseMsg);


                File.WriteAllText("WriteLines.txt", responseMsg);
                var readText = File.ReadAllText(@"C:\Users\PC\source\repos\BinanceWebSocket\BinanceWebSocket\bin\Debug\net7.0\WriteLines.txt");
                var info = JsonConvert.DeserializeObject<FirstObject>(readText);




            }
        }
    }
}

