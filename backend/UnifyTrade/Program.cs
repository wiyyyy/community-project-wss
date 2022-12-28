using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;
using UnifyTrade;
using UnifyTrade.Exchange.Binance;
using UnifyTrade.Exchange.Bybit;
using UnifyTrade.Exchange.Huobi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddSingleton(_ => Channel.CreateUnbounded<BinanceTradeRawData>());
builder.Services.AddHostedService<BinanceTradeReceiver>();
builder.Services.AddHostedService<BinanceTradeSender>();

builder.Services.AddSingleton(_ => Channel.CreateUnbounded<BybitTradeRawData>());
builder.Services.AddHostedService<BybitTradeReceiver>();
builder.Services.AddHostedService<BybitTradeSender>();

builder.Services.AddSingleton(_ => Channel.CreateUnbounded<HuobiTradeRawData>());
builder.Services.AddHostedService<HuobiTradeReceiver>();
builder.Services.AddHostedService<HuobiTradeSender>();

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(options => options.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
app.MapHub<TradeHub>("/aggrtrade");

app.Run();

internal class TradeHub : Hub
{
}