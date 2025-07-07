using DiscountManager.Application.Interfaces;
using DiscountManager.Application.Services;
using DiscountManager.Domain.Interfaces;
using DiscountManager.Infrastructure.Logging;
using DiscountManager.Infrastructure.Persistence;
using DiscountManager.WebApi;

var builder = WebApplication.CreateBuilder(args);

var logPath = Path.Combine(AppContext.BaseDirectory, builder.Configuration["Storage:LogFile"]);
var discountPath = Path.Combine(AppContext.BaseDirectory, builder.Configuration["Storage:DiscountFile"]);

builder.Services.AddSingleton<IFileLogger>(
    _ => new AsyncFileLogger(logPath)
);

builder.Services.AddSingleton<IDiscountRepository>(sp =>
    new JsonDiscountRepository(
        discountPath,
        sp.GetRequiredService<IFileLogger>())
);

builder.Services.AddSingleton<IDiscountService, DiscountCodeService>();
builder.Services.AddSingleton<DiscountWebSocketHandler>();

var app = builder.Build();

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var handler = context.RequestServices.GetRequiredService<DiscountWebSocketHandler>();
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        await handler.ProcessAsync(ws);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();
