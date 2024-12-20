using System.Security.Cryptography.X509Certificates;

namespace Ccs3ClientAppWindowsService;

public class CertificateHelper {
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