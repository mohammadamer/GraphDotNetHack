using Azure.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using MeetingRecordingNotifier.Models;

namespace MeetingRecordingNotifier.KeyVault
{
    public class KeyVaultService : IKeyVaultService
    {
        private string? EncryptionCertificate;
        private string? DecryptionCertificate;
        private string? EncryptionCertificateId;
        private readonly IOptions<KeyVaultOptions> KeyVaultOptions;
        public KeyVaultService(IOptions<KeyVaultOptions> keyVaultOptions)
        {
            KeyVaultOptions = keyVaultOptions;
        }
        public async Task<string> GetEncryptionCertificate()
        {
            // Always renewing the certificate when creating or renewing the subscription so that the certificate
            // can be rotated/changed in key vault without having to restart the application
            await GetCertificateFromKeyVault().ConfigureAwait(false);
            return EncryptionCertificate;
        }

        public async Task<string> GetDecryptionCertificate()
        {
            if (string.IsNullOrEmpty(DecryptionCertificate))
            {
                await this.GetCertificateFromKeyVault().ConfigureAwait(false);
            }

            return DecryptionCertificate;
        }

        public async Task<string> GetEncryptionCertificateId()
        {
            if (string.IsNullOrEmpty(this.EncryptionCertificateId))
            {
                await this.GetCertificateFromKeyVault().ConfigureAwait(false);
            }

            return EncryptionCertificateId;
        }

        private async Task GetCertificateFromKeyVault()
        {
            try
            {
                string keyVaultUri = KeyVaultOptions.Value.KeyVaultUri;
                string tenantId = KeyVaultOptions.Value.TenantId;
                string clientId = KeyVaultOptions.Value.ClientId;
                string clientSecret = KeyVaultOptions.Value.ClientSecret;
                string certificateUrl = KeyVaultOptions.Value.CertificateUrl;

                var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                SecretClient secretClient = new SecretClient(new Uri(keyVaultUri), credential);

                KeyVaultSecret keyVaultCertificatePfx = await secretClient.GetSecretAsync(certificateUrl);

                CertificateClient certificateClient = new CertificateClient(new Uri(keyVaultUri), credential);
                KeyVaultCertificateWithPolicy keyVaultCertificateCer = await certificateClient.GetCertificateAsync(certificateUrl.Replace("/secrets/", "/certificates/", StringComparison.OrdinalIgnoreCase));

                DecryptionCertificate = keyVaultCertificatePfx.Value;
                EncryptionCertificate = Convert.ToBase64String(keyVaultCertificateCer.Cer);
                EncryptionCertificateId = keyVaultCertificatePfx.Properties.Version;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
