using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Samples.WeatherSummaryApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwagger(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration.GetValue<string>("Auth:Issuer");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "weather-api");
    });
});


builder.Services
    .AddHttpClient(
        "weather-api-client",
        client =>
        {
            //client.BaseAddress = new Uri($"{Configuration.GetServiceUri("weather-api")}weatherforecast");
            client.BaseAddress = new Uri("https://localhost:7212/weatherforecast");
        });
    //.ConfigurePrimaryHttpMessageHandler(
    //    () => new HttpClientHandler
    //    {
    //        ServerCertificateCustomValidationCallback =
    //            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    //    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithOAuth(app.Configuration);
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers()
        .RequireAuthorization("ApiScope");
});

app.Run();
