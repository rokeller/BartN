using System;
using System.IO;

namespace BartN;

internal static class StreamExtensions
{
    public static void ReadFull(this Stream input, Span<byte> buffer)
    {
        Span<byte> target = buffer;

        do
        {
            int read = input.Read(target);

            if (read == 0)
            {
                throw new EndOfStreamException();
            }

            target = target.Slice(read);
        } while (target.Length > 0);
    }
}
