using System.DirectoryServices;
using System.Globalization;

namespace Teleopti.Ccc.Infrastructure.ActiveDirectory
{
    /// <summary>
    /// ActiveDirectory repository base functions
    /// </summary>
    public class ActiveDirectoryRepository : IActiveDirectoryRepository
    {

        #region Constant Private Members

        /// <summary>
        /// Default Paging Size for the Search Methods.
        /// </summary>
        private const int DefaultPageSize = 2000;

        #endregion

        #region Interface

        /// <summary>
        /// Returns a DirectoryEntry Object from the Specified key/value Pair.
        /// </summary>
        /// <param name="key">The Property Key.</param>
        /// <param name="value">The Property Value.</param>
        /// <returns>A DirectoryEntry Object.</returns>
        public DirectoryEntry FindDirectoryEntry(string key, string value)
        {
            SearchResult Result;

            using (IDirectorySearcherChannel Searcher = CreateNewDirectorySearcher())
            {
                Searcher.Filter = string.Format(CultureInfo.InvariantCulture, "({0}={1})", key, value);
                Searcher.PageSize = DefaultPageSize;
                Result = Searcher.FindOne();
            }

            return Result.GetDirectoryEntry();

        }

        #endregion

        #region Local Methods

        /// <summary>
        /// Creates the new directory searcher.
        /// </summary>
        /// <remarks>
        /// Method needed for making the method testable.
        /// </remarks>
        protected virtual IDirectorySearcherChannel CreateNewDirectorySearcher()
        {
            return new DirectorySearcherChannel();
        }

        #endregion

    }
}
