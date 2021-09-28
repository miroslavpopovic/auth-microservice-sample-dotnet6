using Microsoft.EntityFrameworkCore;
using Auth;
using Auth.Data;
using Auth.Email;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("auth-db");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, builder =>
    {
        builder.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null);
    }));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AuthUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddEmail(builder.Configuration);
builder.Services.AddRazorPages();

builder.Services.AddIdentityServer()
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryClients(Config.Clients)
    .AddAspNetIdentity<AuthUser>();

builder.Services.AddAuthentication()
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        // We are leaving the default auth scheme
        //options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

        options.ClientId = builder.Configuration["Providers:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Providers:Google:ClientSecret"];
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, "Demo IdentityServer", options =>
    {
        // We are leaving the default auth scheme
        //options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        //options.SignOutScheme = IdentityServerConstants.SignoutScheme;
        options.SaveTokens = true;

        options.Authority = "https://demo.duendesoftware.com";
        options.ClientId = builder.Configuration["Providers:IdentityServerDemo:ClientId"];
        options.ClientSecret = builder.Configuration["Providers:IdentityServerDemo:ClientSecret"];
        options.ResponseType = "code";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

// CORS policy to allow SwaggerUI client
builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "default", policy =>
            {
                policy
                    .WithOrigins("https://localhost:7212")
                    //.WithOrigins(Configuration.GetServiceUri("aurelia-client")!.ToString().TrimEnd('/'))
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("default");

app.UseIdentityServer();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
