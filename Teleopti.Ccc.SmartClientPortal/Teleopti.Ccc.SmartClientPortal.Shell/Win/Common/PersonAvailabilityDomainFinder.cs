using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    /// <summary>
    /// Represents the domain finder for the person availability.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-22
    /// </remarks>
    public class PersonAvailabilityDomainFinder : IDomainFinder
    {
	    private readonly FilteredPeopleHolder _filteredPeopleHolder;
        private PersonAvailabilityRepository _personAvailabilityRepository;

	    /// <summary>
        /// Gets the person rotation repository.
        /// </summary>
        /// <value>The person rotation repository.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-22
        /// </remarks>
        private PersonAvailabilityRepository PersonAvailabilityRepository
        {
            get {
	            return _personAvailabilityRepository ??
	                   (_personAvailabilityRepository =
		                   PersonAvailabilityRepository.DONT_USE_CTOR(_filteredPeopleHolder.GetUnitOfWork));
            }
        }

	    /// <summary>
        /// Initializes a new instance of the <see cref="PersonAvailabilityDomainFinder"/> class.
        /// </summary>
        /// <param name="filteredPeopleHolder">The filtered people holder.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-22
        /// </remarks>
        public PersonAvailabilityDomainFinder(FilteredPeopleHolder filteredPeopleHolder)
        {
            _filteredPeopleHolder = filteredPeopleHolder;
        }

	    /// <summary>
        /// Gets the sorted person collection.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirsha
        /// Created date: 2008-10-22
        /// </remarks>
        private IList<IPerson> getSortedPersonCollection()
        {
            IList<IPerson> collection = _filteredPeopleHolder.FilteredPersonCollection;
            IList<IPerson> sortedData = (from person in collection
                                         orderby person.Name.ToString() ascending
                                         select person).ToList();

            return sortedData;
        }

        /// <summary>
        /// Determines whether [is person already displayed] [the specified person].
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns>
        /// 	<c>true</c> if [is person already displayed] [the specified person]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-22
        /// </remarks>
        private bool IsPersonAlreadyDisplayed(IPerson person)
        {
            return _filteredPeopleHolder.FilteredPersonCollection.Contains(person);
        }

        /// <summary>
        /// Finds the specified search criteria.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-22
        /// </remarks>
        private static bool Find(SearchCriteria searchCriteria, IPerson person)
        {
            bool isFound = false;
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

        /// <summary>
        /// Finds the within child data.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-22
        /// </remarks>
        private bool FindWithinChildData(SearchCriteria searchCriteria, IPerson person)
        {
            bool isFound = false;

            // Gets the data list
            var period = new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MaxValue);

            IList<IPerson> containedCollection = new List<IPerson>();
            containedCollection.Add(person);

            ICollection<IPersonAvailability> availabilityCollection = PersonAvailabilityRepository.Find(containedCollection, period);
            IOrderedEnumerable<IPersonAvailability> sorted = availabilityCollection.OrderByDescending(n2 =>
                                                             n2.StartDate);
            availabilityCollection = sorted.ToList();

            foreach (var availability in availabilityCollection)
            {
                isFound = Find(searchCriteria, availability);

                if (isFound)
                    break;
            }

            return isFound;
        }

        private static bool Find(SearchCriteria searchCriteria, IPersonAvailability pesonAvailability)
        {
            bool isFound;
            string searchText = searchCriteria.SearchText;
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            if (searchCriteria.IsCaseSensitive)
            {
                isFound = pesonAvailability.StartDate.ToString().Contains(searchText) ||
                          pesonAvailability.Availability.Name.Contains(searchText);
            }
            else
            {
                searchText = searchText.ToLower(currentCulture);
                isFound =
                    pesonAvailability.StartDate.ToString().ToLower(currentCulture).Contains(searchText) ||
                    pesonAvailability.Availability.Name.ToLower(currentCulture).Contains(searchText);
            }

            return isFound;
        }

        /// <summary>
        /// Fills the grid.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="fullName">The full name.</param>
        /// <remarks>
        /// Created by: Savani Nirsha
        /// Created date: 2008-10-22
        /// </remarks>
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

        /// <summary>
        /// Gets the index of the current row.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 2008-10-22
        /// </remarks>
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

	    /// <summary>
        /// Finds the data on the persistance which meets the given search criteria and
        /// displays the search results on the given grid control.
        /// </summary>
        /// <param name="grid">Grid to diaply search data</param>
        /// <param name="searchCriteria">Search criteria</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-22
        /// </remarks>
        public void Find(GridControl grid, SearchCriteria searchCriteria)
        {
            IList<IPerson> sortedList = getSortedPersonCollection();

            for (int index = 0; index < sortedList.Count; index++)
            {
                IPerson person = sortedList[index];
                bool isExistsInFilteredList = IsPersonAlreadyDisplayed(person);

                if (isExistsInFilteredList)
                {
                    bool isItemFound = Find(searchCriteria, person);

                    if (!isItemFound)
                    {
                        isItemFound = FindWithinChildData(searchCriteria, person);
                    }

                    if (isItemFound)
                    {
                        FillGrid(grid, person.Name.ToString());
                    }
                }
            }
        }
    }

}
