using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp;

internal static class Utils {
    internal static string GetSha512(string value) {
        using var sha512 = SHA512.Create();
        byte[] valueBytes = Encoding.UTF8.GetBytes(value);
        byte[] hashBytes = sha512.ComputeHash(valueBytes);
        string hashString = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
        return hashString;
    }
}
