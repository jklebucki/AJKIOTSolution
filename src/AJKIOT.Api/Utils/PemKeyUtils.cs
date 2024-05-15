using System.Security.Cryptography.X509Certificates;

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
        public static X509Certificate2 GetCaCertificateFromChain(X509Certificate2 certificate)
        {
            using var chain = new X509Chain();
            chain.Build(certificate);
            foreach (var element in chain.ChainElements)
            {
                // Find the root CA certificate in the chain
                if (element.Certificate.Subject == element.Certificate.Issuer)
                {
                    return element.Certificate;
                }
            }
            return null;
        }

    }
}
