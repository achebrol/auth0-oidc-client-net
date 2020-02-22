using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ping.OidcClient.Tokens
{
    static class JsonWebKeys
    {
        public static async Task<JsonWebKeySet> GetForIssuer(string issuer)
        {
            var metadataAddress = new UriBuilder(issuer) { Path = "/.well-known/openid-configuration" }.Uri.OriginalString;
            var openIdConfiguration = await GetOpenIdConfiguration(metadataAddress);

            var keySet = new JsonWebKeySet();
            //Exclude ECDsa keys as its not supported in .net mono/xamarin
            openIdConfiguration.JsonWebKeySet.Keys
                                .Where(k => !string.IsNullOrEmpty(k.E) &&
                                            !string.IsNullOrEmpty(k.N))
                                .ToList()
                                .ForEach(k => keySet.Keys.Add(k));
            IdentityModelEventSource.ShowPII = true;

            return keySet;
        }

        private static Task<OpenIdConnectConfiguration> GetOpenIdConfiguration(string metadataAddress)
        {
            try
            {
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, new OpenIdConnectConfigurationRetriever());
                return configurationManager.GetConfigurationAsync();
            }
            catch (Exception e)
            {
                throw new IdTokenValidationException($"Unable to retrieve public keys from \"${metadataAddress}\".", e);
            }
        }
    }
}