using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Represnts the domainfinder for the persona accounts.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-20
    /// </remarks>
    public class PersonAccountDomainFinder : IDomainFinder
    {
        private readonly FilteredPeopleHolder _filteredPeopleHolder;

        public PersonAccountDomainFinder(FilteredPeopleHolder filteredPeopleHolder)
        {
            _filteredPeopleHolder = filteredPeopleHolder;
        }

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
                        foreach (var account in _filteredPeopleHolder.AllAccounts[person].AllPersonAccounts())
                        {
                            isItemFound = FindDependOnPersonAccountType(searchCriteria, account);

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

        private static bool FindDependOnPersonAccountType(SearchCriteria searchCriteria, IAccount account)
        {
            bool isItemFound;
            isItemFound = Find(searchCriteria, account);

            return isItemFound;
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

        private static bool Find(SearchCriteria searchCriteria, IAccount account)
        {
            bool isFound;
            string searchText = searchCriteria.SearchText;
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            if (searchCriteria.IsCaseSensitive)
            {
                isFound = account.StartDate.ToShortDateString().Contains(searchText) ||
                          UserTexts.Resources.ResourceManager.GetString(account.GetType().Name).Contains(searchText) ||
                          account.BalanceIn.ToString().Contains(searchText) ||
                          account.Extra.ToString().Contains(searchText) ||
                          account.Accrued.ToString().Contains(searchText) ||
                          account.LatestCalculatedBalance.ToString().Contains(searchText) ||
                          account.BalanceOut.ToString().Contains(searchText) ||
                          account.Remaining.ToString().Contains(searchText);
            }
            else
            {
                searchText = searchText.ToLower(currentCulture);
                isFound =
                    account.StartDate.ToShortDateString().ToLower(currentCulture).Contains(searchText) ||
                    UserTexts.Resources.ResourceManager.GetString(account.GetType().Name).ToLower(currentCulture).
                        Contains(searchText) ||
                    account.BalanceIn.ToString().ToLower(currentCulture).Contains(searchText) ||
                    account.Extra.ToString().ToLower(currentCulture).Contains(searchText) ||
                    account.Accrued.ToString().ToLower(currentCulture).Contains(searchText) ||
                    account.LatestCalculatedBalance.ToString().ToLower(currentCulture).Contains(searchText) ||
                    account.BalanceOut.ToString().ToLower(currentCulture).Contains(searchText) ||
                    account.Remaining.ToString().ToLower(currentCulture).Contains(searchText);
            }

            return isFound;
        }
    }
}
