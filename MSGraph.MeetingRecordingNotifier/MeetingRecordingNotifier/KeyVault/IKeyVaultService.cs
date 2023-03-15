using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingRecordingNotifier.KeyVault
{
    public interface IKeyVaultService
    {
        Task<string> GetEncryptionCertificate();
        Task<string> GetDecryptionCertificate();
        Task<string> GetEncryptionCertificateId();
    }
}
