using System.Threading.Tasks;
using Common.Interfaces;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace BusinessLayer.Services
{
    public class KeyVaultService : IKeyVaultService
    {
        private readonly KeyVaultClient _kvClient;
        private readonly string _kvUrl;

        public KeyVaultService(IConfiguration configuration)
        {
            _kvUrl = $"https://${configuration["KeyVaultName"]}.vault.azure.net";
            _kvClient = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var adCredential = new ClientCredential(configuration["MicrosoftAppId"], configuration["MicrosoftAppPassword"]);
                var authenticationContext = new AuthenticationContext(authority, null);
                return (await authenticationContext.AcquireTokenAsync(resource, adCredential)).AccessToken;
            });

        }

        public async Task<bool> SetSecret(string secretName, string secretValue)
        {
            await _kvClient.SetSecretAsync(_kvUrl, secretName, secretValue);

            return true;
        }

        public async Task<string> GetSecret(string secretName)
        {
            var keyvaultSecret = await _kvClient.GetSecretAsync(_kvUrl, secretName).ConfigureAwait(false);
            return keyvaultSecret.Value;
        }
    }
}
