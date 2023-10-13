using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using BartN.Domain;

namespace BartN;

public class Bart
{
    private readonly LocalContext localContext;
    private readonly IStorageProvider storageProvider;
    private CryptoContext? cryptoContext;

    public Bart(LocalContext localContext, IStorageProvider storageProvider)
    {
        this.localContext = localContext;
        this.storageProvider = storageProvider;
    }

    public async Task InitializeAsync(string password)
    {
        if (null != cryptoContext)
        {
            throw new InvalidOperationException("This context is already initialized.");
        }

        SettingsContext sc = await GetSettingsContextAsync();
        cryptoContext = AesOfbCryptoContext.ForPassword(password, sc.Settings);
    }

    // TODO: This should probably not need to be public.
    public IEnumerable<IndexEntry> GetRemoteEntriesEnumerable()
    {
        ThrowIfUninitialized();
        Debug.Assert(null != cryptoContext);

        return cryptoContext.GetIndexEnumerable(storageProvider);
    }

    public async Task RestoreAsync(IndexEntry entry, bool overwrite)
    {
        ThrowIfUninitialized();
        Debug.Assert(null != cryptoContext);

        using Stream encryptedSourceStream = await storageProvider
            .ReadFileStreamAsync(entry);
        using Stream decryptedSourceStream = cryptoContext
            .DecryptStream(encryptedSourceStream);

        // Decrypt to a temporary file.
        string tempPath = Path.GetTempFileName();

        {
            using Stream targetStream = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);

            await decryptedSourceStream.CopyToAsync(targetStream);
        }

        File.SetLastWriteTimeUtc(tempPath, entry.LastModifiedTimestamp);

        string targetPath = Path.Join(localContext.LocalRootPath, entry.RelPath);
        string? targetDir = Path.GetDirectoryName(targetPath);
        if (null != targetDir)
        {
            Directory.CreateDirectory(targetDir);
        }

        // Now try to move the temporary file to the final location.
        File.Move(tempPath, targetPath, overwrite);
    }

    private async Task<SettingsContext> GetSettingsContextAsync()
    {
        using Stream stream = await storageProvider.ReadSettingsStreamAsync()
            .ConfigureAwait(continueOnCapturedContext: false);
        return SettingsContext.FromStream(stream);
    }

    private void ThrowIfUninitialized()
    {
        if (null == cryptoContext)
        {
            throw new InvalidOperationException("The context must first be initialized.");
        }
    }
}
