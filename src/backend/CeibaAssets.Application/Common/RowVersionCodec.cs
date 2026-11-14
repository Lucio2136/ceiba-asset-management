namespace CeibaAssets.Application.Common;

/// <summary>
/// Convierte el <c>rowversion</c> binario a Base64 para exponerlo en la API.
/// </summary>
public static class RowVersionCodec
{
    public static string Encode(byte[] rowVersion) => Convert.ToBase64String(rowVersion);

    public static bool TryDecode(string? value, out byte[] rowVersion)
    {
        rowVersion = [];
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var buffer = new byte[value.Length];
        if (!Convert.TryFromBase64String(value, buffer, out var written))
        {
            return false;
        }

        rowVersion = buffer[..written];
        return true;
    }
}
