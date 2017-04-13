#region Imports
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Domain finder forthe person schedule periods
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-20
    /// </remarks>
    public class PeopleSchedulePeriodDomainFinder : IDomainFinder
    {
        private readonly FilteredPeopleHolder _filteredPeopleHolder;

        #region Methods - Instance Memeber

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PeopleSchedulePeriodDomainFinder"/> class.
        /// </summary>
        /// <param name="filteredPeopleHolder">The filtered people holder.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-20
        /// </remarks>
        public PeopleSchedulePeriodDomainFinder(FilteredPeopleHolder filteredPeopleHolder)
        {
            _filteredPeopleHolder = filteredPeopleHolder;
        }
        
        #endregion

        #endregion

        #region IDomainFinder Members

        /// <summary>
        /// Finds the data on the persistance which meets the given search criteria and
        /// displays the search results on the given grid control.
        /// </summary>
        /// <param name="grid">Grid to diaply search data</param>
        /// <param name="searchCriteria">Search criteria</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-20
        /// </remarks>
        public void Find(GridControl grid, SearchCriteria searchCriteria)
        {
            IList<IPerson> collection = _filteredPeopleHolder.FilteredPersonCollection;
            IList<IPerson> sortedList = (from person in collection
                                         orderby person.Name.ToString() ascending
                                         select person).ToList();

            for (int index = 0; index < sortedList.Count; index++)
            {
                IPerson person = sortedList[index];
                bool isExistsInFilteredList = false;

                if (collection.Contains(person))
                {
                    isExistsInFilteredList = true;
                }

                if (isExistsInFilteredList)
                {
                    bool isItemFound = Find(searchCriteria, person);

                    if (!isItemFound)
                    {
                        for (int periodIndex = 0; periodIndex < person.PersonSchedulePeriodCollection.Count; periodIndex++)
                        {
                            ISchedulePeriod schedulePeriod = person.PersonSchedulePeriodCollection[periodIndex];
                            isItemFound = Find(searchCriteria, schedulePeriod);

                            if (isItemFound)
                                break;
                        }
                    }

                    if (isItemFound)
                    {
                        FillGrid(grid, person.Name.ToString());
                    }
                }
            }
        }

        #endregion

        #region Helpers

        private static bool Find(SearchCriteria searchCriteria, IPerson person)
        {
            bool isFound;
            CultureInfo culture = CultureInfo.CurrentCulture;
            string searchText = searchCriteria.SearchText;

            if (searchCriteria.IsCaseSensitive)
            {
                isFound = person.Name.FirstName.Contains(searchText) ||
                          person.Name.LastName.Contains(searchText);
            }
            else
            {
                searchText = searchText.ToLower(culture);
                isFound = person.Name.FirstName.ToLower(culture).Contains(searchText) ||
                          person.Name.LastName.ToLower(culture).Contains(searchText);
            }

            return isFound;
        }

        private static bool Find(SearchCriteria searchCriteria, ISchedulePeriod schedulePeriod)
        {
            bool isFound;
            string searchText = searchCriteria.SearchText;
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            if (searchCriteria.IsCaseSensitive)
            {
                isFound = schedulePeriod.DateFrom.Date.ToString(currentCulture).Contains(searchText) ||
                          schedulePeriod.Number.ToString(currentCulture).Contains((searchText)) ||
                          schedulePeriod.PeriodType.ToString().Contains(searchText) ||
                          schedulePeriod.AverageWorkTimePerDay.ToString().Contains(searchText) ||
                          schedulePeriod.GetDaysOff(schedulePeriod.DateFrom).ToString(currentCulture).Contains(searchText);
                          //  || schedulePeriod.PeriodTarget(schedulePeriod.DateFrom).ToString().Contains(searchText);
            }
            else
            {
                searchText = searchText.ToLower(currentCulture);
                isFound =
                    schedulePeriod.DateFrom.Date.ToString(currentCulture).ToLower(currentCulture).Contains(searchText) ||
                    schedulePeriod.Number.ToString(currentCulture).ToLower(currentCulture).Contains((searchText)) ||
                    schedulePeriod.PeriodType.ToString().ToLower(currentCulture).Contains(searchText) ||
                    schedulePeriod.AverageWorkTimePerDay.ToString().ToLower(currentCulture).Contains(searchText) ||
                    schedulePeriod.GetDaysOff(schedulePeriod.DateFrom).ToString(currentCulture).ToLower(currentCulture).
                        Contains(searchText); 
                    // || schedulePeriod.PeriodTarget(schedulePeriod.DateFrom).ToString().ToLower(currentCulture).Contains(searchText);
            }

            return isFound;
        }

        private static void FillGrid(GridControl grid, string fullName)
        {
            // Sets the row index for the grid control
            int rowIndex = GetCurrentRowIndex(grid);

            // Sets the row count of the grid control
            grid.RowCount = rowIndex;

            // Fills the search result table
            grid[rowIndex, 1].Text = fullName;
            grid[rowIndex, 2].Text = string.Empty;
            grid[rowIndex, 3].Text = string.Empty;
        }

        private static int GetCurrentRowIndex(GridControl grid)
        {
            // Sets the row index for the grid control
            int rowIndex = grid.RowCount;

            if ((rowIndex > 0) && (!string.IsNullOrEmpty(grid[rowIndex, 1].Text)))
            {
                rowIndex += 1;
            }

            return rowIndex;
        }

        #endregion
    }
}
