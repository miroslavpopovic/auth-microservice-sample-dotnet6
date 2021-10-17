using Auth.Data;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth;

public static class Config
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("weather-api", "Weather API"),
            new ApiScope("weather-summary-api", "Weather Summary API")
        };

    public static IEnumerable<IdentityResource> IdentityResources => new List<IdentityResource>
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile()
    };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // Machine to machine client
            new Client
            {
                ClientId = "weather-api-console-client",

                // secret for authentication
                ClientSecrets = { new Secret("secret".Sha256()) },

                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                // scopes that client has access to
                AllowedScopes = { "weather-api" }
            },

            // Machine to machine client
            new Client
            {
                ClientId = "weather-api-worker-client",

                // secret for authentication
                ClientSecrets = { new Secret("secret".Sha256()) },

                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                // scopes that client has access to
                AllowedScopes = { "weather-api" }
            },

            // Machine to machine client
            new Client
            {
                ClientId = "weather-summary-api-console-client",

                // secret for authentication
                ClientSecrets = { new Secret("secret".Sha256()) },

                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                // scopes that client has access to
                AllowedScopes = { "weather-summary-api" }
            },

            // Machine to machine client
            new Client
            {
                ClientId = "weather-apis-client",

                // secret for authentication
                ClientSecrets = { new Secret("secret".Sha256()) },

                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                // scopes that client has access to
                AllowedScopes = { "weather-api", "weather-summary-api" }
            },

            // Interactive web client that's using ASP.NET Core MVC
            new Client
            {
                ClientId = "weather-api-mvc-client",

                // secret for authentication
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                RequireConsent = true,
                RequirePkce = true,

                // where to redirect to after login
                RedirectUris = { "https://localhost:7214/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:7214/signout-callback-oidc" },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "weather-api",
                    "weather-summary-api"
                },

                AllowOfflineAccess = true
            },

            new Client
            {
                ClientId = "bff-client",

                // secret for authentication
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                // where to redirect to after login
                RedirectUris = { "https://localhost:7215/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:7215/signout-callback-oidc" },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "weather-api"
                }
            },

            new Client
            {
                ClientId = "wpf-client",
                ClientName = "WPF Client",

                AllowedGrantTypes = GrantTypes.DeviceFlow,
                RequireClientSecret = false,

                AlwaysIncludeUserClaimsInIdToken = true,
                AllowOfflineAccess = true,

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "weather-api"
                }
            },

            new Client
            {
                ClientId = "auth-admin-client",

                // secret for authentication
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                RequireConsent = true,
                RequirePkce = true,

                // where to redirect to after login
                RedirectUris = { "https://localhost:7211/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:7211/signout-callback-oidc" },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile
                },

                AllowOfflineAccess = true
            }
        };

    public static void InitializeDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();

        var appContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        appContext.Database.Migrate();

        var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        context.Database.Migrate();

        if (!context.Clients.Any())
        {
            foreach (var client in Clients)
            {
                context.Clients.Add(client.ToEntity());
            }
            context.SaveChanges();
        }

        if (!context.IdentityResources.Any())
        {
            foreach (var resource in IdentityResources)
            {
                context.IdentityResources.Add(resource.ToEntity());
            }
            context.SaveChanges();
        }

        if (!context.ApiScopes.Any())
        {
            foreach (var resource in ApiScopes)
            {
                context.ApiScopes.Add(resource.ToEntity());
            }
            context.SaveChanges();
        }
    }
}
