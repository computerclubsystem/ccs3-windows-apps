using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Ccs3ClientAppWindowsService;

public class CertificateHelper {
    private X509Certificate2 _localServiceCert;

    public string? EncryptWithLocalServiceCertificate(string data) {
        byte[] encryptedBuffer = new byte[2048];
        Span<byte> encryptedSpan = new(encryptedBuffer);
        byte[] arrayToEncrypt = Encoding.UTF8.GetBytes(data);
        ReadOnlySpan<byte> spanToEncrypt = new(arrayToEncrypt);
        bool isEncrypted = _localServiceCert.GetRSAPrivateKey().TryEncrypt(spanToEncrypt, encryptedSpan, RSAEncryptionPadding.OaepSHA3_256, out int bytesWritten);
        if (isEncrypted) {
            string result = Encoding.UTF8.GetString(encryptedBuffer, 0, bytesWritten);
            return result;
        } else {
            return null;
        }
    }

    public void SetLocalServiceCertificate(X509Certificate2 cert) {
        _localServiceCert = cert;
    }

    public GetPersonalCertificateIssuedByTrustedRootCAResult GetPersonalCertificateIssuedByTrustedRootCA(string issuerCertThumbprint, string certCNToSearchFor) {
        GetPersonalCertificateIssuedByTrustedRootCAResult result = new();
        using X509Store trustedRootStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine, OpenFlags.ReadOnly);
        string uppedcasedIssuerThumbprint = issuerCertThumbprint.ToUpper();
        var caCert = trustedRootStore.Certificates.Find(X509FindType.FindByThumbprint, issuerCertThumbprint, false).FirstOrDefault();
        if (caCert is null) {
            result.IssuerFound = false;
            return result;
        }

        result.IssuerFound = true;
        string? caCertNameString = caCert.SubjectName.ToString();
        using X509Store personalStore = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.ReadOnly);
        result.PersonalCertificate = personalStore.Certificates.FirstOrDefault(cert => {
            if (cert.Issuer != caCert.Subject) {
                return false;
            }
            string certSimpleName = cert.GetNameInfo(X509NameType.SimpleName, false);
            return string.Equals(certSimpleName, certCNToSearchFor, StringComparison.OrdinalIgnoreCase);
        });
        return result;
    }
}

public class GetPersonalCertificateIssuedByTrustedRootCAResult {
    public X509Certificate2? PersonalCertificate { get; set; }
    public bool IssuerFound { get; set; }
}