using TwitchFollowers.Domain.Model.Configuration;
using TwitchFollowers.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddScoped<TwitchService>();

builder.Services
    .AddOptions<Urls>()
    .Configure<IConfiguration>((settings, config) =>
    {
        config.GetSection("Urls").Bind(settings);
    });

builder.Services
    .AddOptions<Twitch>()
    .Configure<IConfiguration>((settings, config) =>
    {
        config.GetSection("TwitchToken").Bind(settings);
    });

builder.Services
    .AddOptions<TagsConfig>()
    .Configure<IConfiguration>((settings, config) =>
    {
        config.GetSection("TagsConfig").Bind(settings);
    });

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
