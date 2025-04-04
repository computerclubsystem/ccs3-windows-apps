using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientAppBootstrapWindowsService;

public class HttpDownloader {
    public async Task<DownloadStringResult> DownloadStringAsync(Uri uri, CancellationToken stoppingToken) {
        DownloadStringResult result = new();
        try {
            using HttpClientHandler httpClientHandler = new();
            httpClientHandler.ServerCertificateCustomValidationCallback = ServerCertificateCustomValidationCallback;
            using HttpClient httpClient = new(httpClientHandler, true);
            string downloadedString = await httpClient.GetStringAsync(uri, stoppingToken);
            result.Result = downloadedString;
            result.Success = true;
        } catch (Exception ex) {
            result.Success = false;
            result.Exception = ex;
        }
        return result;
    }

    public async Task<DownloadAndSaveToFileResult> DownloadAndSaveToFileAsync(Uri uri, string targetFilePath, CancellationToken stoppingToken) {
        DownloadAndSaveToFileResult result = new();
        try {
            using HttpClientHandler httpClientHandler = new();
            httpClientHandler.ServerCertificateCustomValidationCallback = ServerCertificateCustomValidationCallback;
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

    private bool ServerCertificateCustomValidationCallback(
        HttpRequestMessage reqMessage,
        X509Certificate2? cert,
        X509Chain? chain,
        SslPolicyErrors policyErrors
    ) {
        // TODO: Validate server certificate
        return true;
    }
}

public class DownloadAndSaveToFileResult {
    public bool Success { get; set; }
    public Exception? Exception { get; set; }
}

public class DownloadStringResult {
    public bool Success { get; set; }
    public string? Result { get; set; }
    public Exception? Exception { get; set; }
}