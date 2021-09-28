using Duende.IdentityServer.Models;

namespace Auth;

public static class Config
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("weather-api", "Weather API")
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
            }
        };
}
