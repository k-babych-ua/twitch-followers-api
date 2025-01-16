using TwitchFollowers.Domain.Model.Configuration;
using TwitchFollowers.Domain.Services;

const string _originPolicy = "myAllowPolicy";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        _originPolicy,
        policy =>
        {
            policy
                .WithOrigins("https://www.twitch.tv")
                .WithMethods("POST");
        });
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<TwitchService>();
builder.Services.AddScoped<FollowingService>();

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
        var tagsSection = config.GetSection("TagsConfig");
        if (tagsSection != null)
        {
            settings.Green = tagsSection.GetSection("Green")?.Value?.Split(",");
            settings.Red = tagsSection.GetSection("Red")?.Value?.Split(",");
        }
    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(_originPolicy);
app.MapControllers();

app.Run();
