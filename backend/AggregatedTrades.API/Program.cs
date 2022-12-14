using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddSingleton(_ => Channel.CreateUnbounded<byte[]>());

builder.Services.AddHostedService<BackgroundTradeSender>();
builder.Services.AddHostedService<BackgroundTradeReceiver>();
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