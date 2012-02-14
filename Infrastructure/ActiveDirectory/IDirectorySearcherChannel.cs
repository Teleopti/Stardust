using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Infrastructure.ActiveDirectory
{
    /// <summary>
    /// Methods used by Infrastructure from DirectorySearcher class. 
    /// </summary>
    public interface IDirectorySearcherChannel : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating the Lightweight Directory Access Protocol (LDAP) format filter string.
        /// </summary>
        string Filter{ set; get;}


        /// <summary>
        /// Gets or sets a value indicating the page size in a paged search.
        /// </summary>
        /// <value>The maximum number of objects the server can return in a paged search. The default is zero, which means do not do a paged search.</value>
        int PageSize { set; get; }


        /// <summary>
        /// Executes the search and returns only the first entry that is found.
        /// </summary>
        /// <returns></returns>
        SearchResult FindOne();


        /// <summary>
        /// Gets a value indicating the list of properties to retrieve during the search.
        /// </summary>
        /// <value>
        /// A System.Collections.Specialized.StringCollection object that contains the set of properties 
        /// to retrieve during the search.The default is an empty System.Collections.Specialized.StringCollection,
        /// which retrieves all properties.
        /// </value>
        StringCollection PropertiesToLoad { get;}


        /// <summary>
        /// Executes the search and returns a collection of the entries that are found.
        /// </summary>
        /// <returns>
        /// A System.DirectoryServices.SearchResultCollection object that contains the results of the search.
        /// </returns>
        SearchResultCollection FindAll();


        /// <summary>
        /// Gets or sets a value indicating the maximum number of objects that the server returns in a search.
        /// </summary>
        /// <value>
        /// The maximum number of objects that the server returns in a search. The default value is zero, 
        /// which means to use the server-determined default size limit of 1000 entries.
        /// </value>
        int SizeLimit { set; get; }


        /// <summary>
        /// Gets or sets a value indicating the property on which the results are sorted.
        /// </summary>
        /// <value>
        /// A System.DirectoryServices.SortOption object that specifies the property and direction that the search
        /// results should be sorted on.
        /// </value>
        SortOption Sort { set; get; }

        /// <summary>
        /// Gets or sets a value indicating the node in the Active Directory Domain Services hierarchy where the 
        /// search starts.
        /// </summary>
        /// <value>
        /// The System.DirectoryServices.DirectoryEntry object in the Active Directory Domain Services hierarchy where
        /// the search starts. The default is a null reference (Nothing in Visual Basic).
        /// </value>
        DirectoryEntry SearchRoot { set; get; }

    }
}
