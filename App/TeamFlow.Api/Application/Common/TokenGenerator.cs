using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace App.Application.Common;
public static class TokenGenerator
{
    public static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}

