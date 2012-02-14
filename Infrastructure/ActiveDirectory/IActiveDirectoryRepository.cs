using System.DirectoryServices;

namespace Teleopti.Ccc.Infrastructure.ActiveDirectory
{
    /// <summary>
    /// ActiveDirectory base repository functions interface
    /// </summary>
    public interface IActiveDirectoryRepository
    {
        /// <summary>
        /// Finds the directory entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        DirectoryEntry FindDirectoryEntry(string key, string value);
    }
}
