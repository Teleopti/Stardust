using System.Collections.Specialized;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices;
using Teleopti.Ccc.Infrastructure.ActiveDirectory;

namespace Teleopti.Ccc.InfrastructureTest.ActiveDirectory
{
    public class DirectorySearcherChannelStub : IDirectorySearcherChannel
    {
        #region IDirectorySearcherChannel Members

        /// <summary>
        /// Gets or sets a value indicating the Lightweight Directory Access Protocol (LDAP) format filter string.
        /// </summary>
        /// <value></value>
        public string Filter
        {
            get
            {
                return string.Empty;
            }
            set
            {
                //
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the page size in a paged search.
        /// </summary>
        /// <value>
        /// The maximum number of objects the server can return in a paged search. The default is zero, which means do not do a paged search.
        /// </value>
        public int PageSize
        {
            get
            {
                return 0;
            }
            set
            {
                //
            }
        }

        /// <summary>
        /// Executes the search and returns only the first entry that is found.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public SearchResult FindOne()
        {
            throw new COMException("The specified domain either does not exist or could not to be contacted.");
        }

        /// <summary>
        /// Gets a value indicating the list of properties to retrieve during the search.
        /// </summary>
        /// <value>
        /// A System.Collections.Specialized.StringCollection object that contains the set of properties
        /// to retrieve during the search.The default is an empty System.Collections.Specialized.StringCollection,
        /// which retrieves all properties.
        /// </value>
        public StringCollection PropertiesToLoad
        {
            get { return new StringCollection(); }
        }

        /// <summary>
        /// Executes the search and returns a collection of the entries that are found.
        /// </summary>
        /// <returns>
        /// A System.DirectoryServices.SearchResultCollection object that contains the results of the search.
        /// </returns>
        public SearchResultCollection FindAll()
        {
            return null;
        }

        /// <summary>
        /// Gets or sets a value indicating the maximum number of objects that the server returns in a search.
        /// </summary>
        /// <value>
        /// The maximum number of objects that the server returns in a search. The default value is zero,
        /// which means to use the server-determined default size limit of 1000 entries.
        /// </value>
        public int SizeLimit
        {
            get
            {
                return 0;
            }
            set
            {
                //;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the property on which the results are sorted.
        /// </summary>
        /// <value>
        /// A System.DirectoryServices.SortOption object that specifies the property and direction that the search
        /// results should be sorted on.
        /// </value>
        public SortOption Sort
        {
            get
            {
                return null;
            }
            set
            {
                //;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the node in the Active Directory Domain Services hierarchy where the
        /// search starts.
        /// </summary>
        /// <value>
        /// The System.DirectoryServices.DirectoryEntry object in the Active Directory Domain Services hierarchy where
        /// the search starts. The default is a null reference (Nothing in Visual Basic).
        /// </value>
        public DirectoryEntry SearchRoot
        {
            get
            {
                return null;
            }
            set
            {
                //
            }
        }

        #endregion

        #region IDisposable Members

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes the specified disposing.
        /// </summary>
        /// <param name="disposing">if set to <c>true</c> [disposing].</param>
        protected virtual void Dispose(bool disposing)
        {
            //
        }

        #endregion

        #endregion
    }
}
