using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.Security.ActiveDirectory
{
    /// <summary>
    /// ActiveDirectoryUserRepository function interfaces
    /// </summary>
    public interface IActiveDirectoryUserRepository
    {
        /// <summary>
        /// Finds the token groups that the user belongs to.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        Collection<ActiveDirectoryGroup> FindTokenGroups(ActiveDirectoryUser user);
        /// <summary>
        /// Finds the active directory user.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        ActiveDirectoryUser FindUser(string key, string value);
        /// <summary>
        /// Finds active directory users.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        Collection<ActiveDirectoryUser> FindUsers(string key, string value);

        //Collection<ActiveDirectoryUser> FindUsers(string Filter, DirectoryEntry SearchRoot, int SizeLimit, SortOption Sort);
    }
}
