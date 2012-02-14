using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.Security.ActiveDirectory
{
    /// <summary>
    /// Interface for ActiveDirectoryGroupRepository
    /// </summary>
    public interface IActiveDirectoryGroupRepository
    {
        /// <summary>
        /// Finds an active directory group.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        ActiveDirectoryGroup FindGroup(string key, string value);
        /// <summary>
        /// Finds active directory groups.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        Collection<ActiveDirectoryGroup> FindGroups(string key, string value);

        //Collection<ActiveDirectoryGroup> FindGroups(string Filter, DirectoryEntry SearchRoot, int SizeLimit, SortOption Sort);
    }
}
