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
    /// Represents the domain finder for the person rotation.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-21
    /// </remarks>
    public class PersonRotationDomainFinder : IDomainFinder
    {
	    private readonly FilteredPeopleHolder _filteredPeopleHolder;
        private PersonRotationRepository _personRotationRepository;

	    /// <summary>
        /// Initializes a new instance of the <see cref="PersonRotationDomainFinder"/> class.
        /// </summary>
        /// <param name="filteredPeopleHolder">The filtered people holder.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-21
        /// </remarks>
        public PersonRotationDomainFinder(FilteredPeopleHolder filteredPeopleHolder)
        {
            _filteredPeopleHolder = filteredPeopleHolder;
        }

	    private bool IsPersonAlreadyDisplayed(IPerson person)
        {
            return _filteredPeopleHolder.FilteredPersonCollection.Contains(person);
        }

        private IList<IPerson> getSortedPersonCollection()
        {
            IList<IPerson> collection = _filteredPeopleHolder.FilteredPersonCollection;
            IList<IPerson> sortedData = (from person in collection
                                         orderby person.Name.ToString() ascending
                                         select person).ToList();

            return sortedData;
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

        private bool FindWithinChildData(SearchCriteria searchCriteria,IPerson person)
        {
            bool isFound = false;

            IList<IPersonRotation> rotationCollection = PersonRotationRepository.Find(person);

            IOrderedEnumerable<IPersonRotation> sorted = rotationCollection.OrderByDescending(n2 => n2.StartDate);
            rotationCollection = sorted.ToList();

            foreach (IPersonRotation rotation in rotationCollection)
            {
                isFound = Find(searchCriteria, rotation);

                if (isFound)
                    break;
            }

            return isFound;
        }

        private static bool Find(SearchCriteria searchCriteria, IPersonRotation personRotation)
        {
            bool isFound = false;
            string searchText = searchCriteria.SearchText;
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            if (searchCriteria.IsCaseSensitive)
            {
                isFound = personRotation.StartDate.ToString().Contains(searchText) ||
                          personRotation.Rotation.Name.Contains(searchText);
            }
            else
            {
                searchText = searchText.ToLower(currentCulture);
                isFound = personRotation.StartDate.ToString().ToLower(currentCulture).Contains(searchText) ||
                          personRotation.Rotation.Name.ToLower(currentCulture).Contains(searchText);
            }

            return isFound;
        }

        private PersonRotationRepository PersonRotationRepository
        {
            get {
	            return _personRotationRepository ??
	                   (_personRotationRepository = PersonRotationRepository.DONT_USE_CTOR(_filteredPeopleHolder.GetUnitOfWork));
            }
        }

	    /// <summary>
        /// Finds the data on the persistance which meets the given search criteria and
        /// displays the search results on the given grid control.
        /// </summary>
        /// <param name="grid">Grid to diaply search data</param>
        /// <param name="searchCriteria">Search criteria</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-21
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
