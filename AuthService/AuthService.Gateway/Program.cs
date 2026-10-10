using AuthService.Security;

var builder = WebApplication.CreateBuilder(args);

// Шлюз проверяет JWT и права ДО проксирования: неавторизованные запросы
// до внутренних сервисов не доходят.
builder.Services.AddAuthServiceSecurity(builder.Configuration);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();
