using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using BartN.Domain;

namespace BartN;

public abstract class CryptoContext
{
    public string Password { get; }
    public byte[] Salt { get; }
    public byte[] Key { get; protected set; }

    protected CryptoContext(string password, byte[] salt)
    {
        Password = password;
        Salt = salt;

        DeriveKey();
    }

    public abstract Stream DecryptStream(Stream sourceStream);

    public IEnumerable<IndexEntry> GetIndexEnumerable(string path)
    {
        // In v1 of bart, the index file is encrypted after gzipping the data.
        // That is, we decrypt before g-unzipping.
        using Stream input = File.OpenRead(path);
        using Stream decrypted = DecryptStream(input);
        using GZipStream gzip = new GZipStream(decrypted, CompressionMode.Decompress);

        foreach (IndexEntry entry in GetIndexEnumerable(gzip))
        {
            yield return entry;
        }
    }

    public IEnumerable<IndexEntry> GetIndexEnumerable(IStorageProvider storageProvider)
    {
        // In v1 of bart, the index file is encrypted after gzipping the data.
        // That is, we decrypt before g-unzipping.
        using Stream input = storageProvider
            .ReadIndexStreamAsync()
            .ConfigureAwait(false)
            .GetAwaiter().GetResult();
        using Stream decrypted = DecryptStream(input);
        using GZipStream gzip = new GZipStream(decrypted, CompressionMode.Decompress);

        foreach (IndexEntry entry in GetIndexEnumerable(gzip))
        {
            yield return entry;
        }
    }

    protected abstract void DeriveKey();

    private IEnumerable<IndexEntry> GetIndexEnumerable(Stream index)
    {
        // Each index entry is prefixed with a little-endian 4-byte length
        // indicator.
        Span<byte> size = stackalloc byte[4];
        IndexEntry? entry;

        while (null != (entry = ReadEntry(index)))
        {
            yield return entry;
        }
    }

    private static IndexEntry? ReadEntry(Stream input)
    {
        Span<byte> bufSize = stackalloc byte[4];
        try
        {
            input.ReadFull(bufSize);
        }
        catch (EndOfStreamException)
        {
            return null;
        }

        int recordLen = BitConverter.ToInt32(bufSize);
        Span<byte> bufEntry = stackalloc byte[recordLen];

        input.ReadFull(bufEntry);

        return IndexEntry.Parser.ParseFrom(bufEntry);
    }
}
