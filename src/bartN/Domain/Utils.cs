using System;
using System.Text;

namespace BartN.Domain;

internal static class Utils
{
    private static readonly Encoding golangUtf8 = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false);

    public static int GetBytesForGolangString(string input, Span<byte> buffer)
    {
        return golangUtf8.GetBytes(input, buffer);
    }
}
