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
        public static X509Certificate2 LoadCertificate(string filePath)
        {
            try
            {
                string pemContents = File.ReadAllText(filePath);
                return new X509Certificate2(ConvertFromPemToDer(pemContents));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading certificate: {ex.Message}");
                throw;
            }
        }

        private static byte[] ConvertFromPemToDer(string pemContents)
        {
            const string header = "-----BEGIN CERTIFICATE-----";
            const string footer = "-----END CERTIFICATE-----";

            int start = pemContents.IndexOf(header, StringComparison.Ordinal) + header.Length;
            int end = pemContents.IndexOf(footer, start, StringComparison.Ordinal);

            string base64 = pemContents[start..end];
            return Convert.FromBase64String(base64);
        }
    }
}
