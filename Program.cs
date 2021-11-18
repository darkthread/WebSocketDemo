using System.Net.WebSockets;
using WebSocketDemo.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

//加入WebSocket處理服務
builder.Services.AddSingleton<WebSocketHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//加入 WebSocket 功能
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using (WebSocket ws = await context.WebSockets.AcceptWebSocketAsync())
            {
                var wsHandler = context.RequestServices.GetRequiredService<WebSocketHandler>();
                await wsHandler.ProcessWebSocket(ws);
            }
        }
        else 
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
    }
    else await next();
});

app.UseRouting();

app.UseAuthorization();


app.MapRazorPages();



app.Run();
