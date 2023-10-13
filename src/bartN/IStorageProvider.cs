using System.IO;
using System.Threading.Tasks;
using BartN.Domain;

namespace BartN;

public interface IStorageProvider
{
    #region Reading

    /// <summary>
    /// Reads the (unencrypted) stream of settings for the backup storage.
    /// </summary>
    /// <returns>
    /// </returns>
    Task<Stream> ReadSettingsStreamAsync();

    /// <summary>
    /// Reads the encrypted stream representing the backup file index.
    /// </summary>
    /// <returns>
    /// </returns>
    Task<Stream> ReadIndexStreamAsync();

    /// <summary>
    /// Reads the encrypted stream of the backup file for the given hash.
    /// </summary>
    /// <param name="entry">
    /// </param>
    /// <returns>
    /// </returns>
    Task<Stream> ReadFileStreamAsync(IndexEntry entry);

    #endregion

    #region Writing

    #endregion

    #region Deleting

    Task DeleteSettingsAsync();
    Task DeleteIndexAsync();
    Task DeleteFileAsync(IndexEntry entry);

    #endregion
}
