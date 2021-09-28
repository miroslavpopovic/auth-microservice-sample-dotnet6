using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Polly;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = "https://localhost:7210";

        options.ClientId = "weather-api-mvc-client";
        options.ClientSecret = "secret";
        options.ResponseType = "code";

        options.SaveTokens = true;

        options.Scope.Add("weather-api");
        options.Scope.Add("weather-summary-api");
        options.Scope.Add("offline_access");
        options.Scope.Add("profile");

        options.GetClaimsFromUserInfoEndpoint = true;
    });


// Register and configure Token Management and Weather API HTTP clients for DI
// This is using a separate Client to access API using Client Credentials
builder.Services
    .AddAccessTokenManagement(
        options =>
        {
            options.Client.Clients.Add(
                "auth", new ClientCredentialsTokenRequest
                {
                    //Address = $"{Configuration.GetServiceUri("auth")}connect/token",
                    Address = "https://localhost:7210/connect/token",
                    ClientId = "weather-apis-client",
                    ClientSecret = "secret",
                    Scope = "weather-api"
                });
        })
    .ConfigureBackchannelHttpClient()
    .ConfigurePrimaryHttpMessageHandler(
        () => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
    .AddTransientHttpErrorPolicy(
        policy =>
            policy.WaitAndRetryAsync(
                new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                }));

builder.Services
    .AddClientAccessTokenHttpClient(
        "weather-api-client",
        configureClient: client =>
        {
            //client.BaseAddress = new Uri(Configuration.GetServiceUri("weather-api")!.ToString());
            client.BaseAddress = new Uri("https://localhost:7212/");
        });
    //.ConfigurePrimaryHttpMessageHandler(
    //    () => new HttpClientHandler
    //    {
    //        ServerCertificateCustomValidationCallback =
    //            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    //    });

builder.Services
    .AddClientAccessTokenHttpClient(
        "weather-summary-api-client",
        configureClient: client =>
        {
            //client.BaseAddress = new Uri(Configuration.GetServiceUri("weather-summary-api")!.ToString());
            client.BaseAddress = new Uri("https://localhost:7213/");
        });
    //.ConfigurePrimaryHttpMessageHandler(
    //    () => new HttpClientHandler
    //    {
    //        ServerCertificateCustomValidationCallback =
    //            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    //    });

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute()
        .RequireAuthorization();
});

app.Run();
