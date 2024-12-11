using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientAppBootstrapWindowsService;

public class HttpDownloader {
    public async Task<DownloadAndSaveToFileResult> DownloadAndSaveToFileAsync(Uri uri, string targetFilePath, CancellationToken stoppingToken) {
        DownloadAndSaveToFileResult result = new();
        try {
            using HttpClientHandler httpClientHandler = new();
            httpClientHandler.ServerCertificateCustomValidationCallback = (
                HttpRequestMessage reqMessage,
                X509Certificate2? cert,
                X509Chain? chain,
                SslPolicyErrors policyErrors
            ) => {
                // TODO: Validate server certificate
                return true;
            };
            using HttpClient httpClient = new(httpClientHandler, true);
            using Stream responseStream = await httpClient.GetStreamAsync(uri, stoppingToken);
            using FileStream outputFileStream = new(targetFilePath, FileMode.Create, FileAccess.Write);
            await responseStream.CopyToAsync(outputFileStream, stoppingToken);
            await responseStream.FlushAsync(stoppingToken);
            await outputFileStream.FlushAsync(stoppingToken);
            result.Success = true;
        } catch (Exception ex) {
            result.Success = false;
            result.Exception = ex;
        }

        return result;
    }
}

public class DownloadAndSaveToFileResult {
    public bool Success { get; set; }
    public Exception? Exception { get; set; }
}