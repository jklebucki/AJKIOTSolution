using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.OpenSsl;

namespace AJKIOT.Api.Utils
{
    public static class PemKeyUtils
    {
        public static X509Certificate2 LoadCertificate(string certPath, string keyPath)
        {
            var certPem = File.ReadAllText(certPath);
            var keyPem = File.ReadAllText(keyPath);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(keyPem.ToCharArray());

            return X509Certificate2.CreateFromPem(certPem, keyPem);
        }

    }
}
