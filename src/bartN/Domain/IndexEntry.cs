using System;
using System.Security.Cryptography;

namespace BartN.Domain;

partial class IndexEntry
{
    private ReadOnlyMemory<byte>? hash;

    public ReadOnlySpan<byte> Hash
    {
        get
        {
            if (!hash.HasValue)
            {
                Span<byte> relPath = stackalloc byte[1024];
                int relPathSize = Utils.GetBytesForGolangString(RelPath, relPath);

                using SHA1 sha1 = SHA1.Create();
                Span<byte> hashBuffer = stackalloc byte[sha1.HashSize / 8];
                relPath = relPath.Slice(0, relPathSize);

                if (!sha1.TryComputeHash(relPath, hashBuffer, out _))
                {
                    throw new InvalidOperationException(
                        "Failed to comput hash for relative path.");
                }

                hash = hashBuffer.ToArray();
            }

            return hash.Value.Span;
        }
    }

    public DateTime LastModifiedTimestamp
    {
        get => DateTime.UnixEpoch.AddSeconds(LastModified);
    }
}
