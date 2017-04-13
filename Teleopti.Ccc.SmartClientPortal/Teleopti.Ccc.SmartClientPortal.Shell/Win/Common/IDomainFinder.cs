using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Represents the IDomainFinder interface.
    /// Use to find the data on the persistance.
    /// </summary>
    public interface IDomainFinder
    {
        /// <summary>
        /// Finds the data on the persistance which meets the given search criteria and 
        /// displays the search results on the given grid control.
        /// </summary>
        /// <param name="grid">Grid to diaply search data</param>
        /// <param name="searchCriteria">Search criteria</param>
        void Find(GridControl grid, SearchCriteria searchCriteria);
    }
}
