using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Represetns the People doamin finder
    /// </summary>
    /// <remarks>
    /// Created By: madhurangap
    /// Created Date: 09-07-2008
    /// </remarks>
    public class PeopleDomainFinder : IDomainFinder
    {
        private FilteredPeopleHolder _filterPeopleHolder;

        #region Methods - Instance Memeber

        #region Contructor
        public PeopleDomainFinder(FilteredPeopleHolder filterPeopleHolder)
        {
            _filterPeopleHolder = filterPeopleHolder;
        }
        #endregion


        #region Methods - Instance Member - PeopleDomainFinder Members

        /// <summary>
        /// Finds the people data that meets the search criteria given.
        ///  and fills the search results to the given grid
        /// </summary>
        /// <param name="grid">Grid to display the search data</param>
        /// <param name="searchCriteria">Search criteria</param>
        public void Find(GridControl grid, SearchCriteria searchCriteria)
        {
            // Loads the full people data collection
            //PeopleWorksheet.StateHolder.LoadFullPeople();

            if ((_filterPeopleHolder.PersonCollection != null) &&
                (_filterPeopleHolder.FilteredPersonCollection != null))
            {
                // Gets the full person collection
                IList<PersonGeneralModel> personList = new List<PersonGeneralModel>(
                    _filterPeopleHolder.PeopleGridData);
                // Gets the filterred person collection
                IList<PersonGeneralModel> personFilteredList = new List<PersonGeneralModel>(
                    _filterPeopleHolder.FilteredPeopleGridData);

                // Gets the not filtered collection
                IEnumerable<PersonGeneralModel> personsNotFiltered = personList.Except(personFilteredList);
                IList<PersonGeneralModel> personsToBeSearch = personsNotFiltered.ToList();

                if ((personsToBeSearch != null) && (!string.IsNullOrEmpty(searchCriteria.SearchText)))
                {
                    // Gets the search results that meets the search criteria
                    IEnumerable<PersonGeneralModel> searchResutls = Search(personsToBeSearch, searchCriteria);

                    // Fills the searh results to teh grid control
                    FillGrid(grid, searchResutls);
                }
            }
        }

        /// <summary>
        /// Searchs withing the given person data list using the given search criteria.
        /// </summary>
        /// <param name="personsToBeSearch">Person list to search</param>
        /// <param name="searchCriteria">Search criteria</param>
        /// <returns>Search result as a person collection</returns>
        private static IEnumerable<PersonGeneralModel> Search(IEnumerable<PersonGeneralModel> personsToBeSearch,
            SearchCriteria searchCriteria)
        {
            // Holds the person search results
            IEnumerable<PersonGeneralModel> personQuery;

            if (searchCriteria.IsCaseSensitive)
            {
                // Searches within the given list considering the case(Upper/Lower) of the given search text
                personQuery =//
                    from
                        person in personsToBeSearch
                    where
                        person.FirstName.Contains(searchCriteria.SearchText) ||
                        person.LastName.Contains(searchCriteria.SearchText) ||
                        person.Email.Contains(searchCriteria.SearchText) ||
                        person.EmployeeNumber.Contains(searchCriteria.SearchText) ||
                        person.Note.Contains(searchCriteria.SearchText) ||
                        person.CultureInfo.DisplayName.Contains(searchCriteria.SearchText) ||
                        person.TimeZone.Contains(searchCriteria.SearchText) ||
                        person.LogOnName.Contains(searchCriteria.SearchText) ||
                        person.ApplicationLogOnName.Contains(searchCriteria.SearchText)
                    select person;
            }
            else
            {
                // Searches within the given list without considering the case(Upper/Lower) of the given search text				
                personQuery =
                    from
                        person in personsToBeSearch
                    where
                        person.FirstName.ToLower(CultureInfo.CurrentCulture).Contains(searchCriteria.SearchText.ToLower(CultureInfo.CurrentCulture)) ||
                        person.LastName.ToLower(CultureInfo.CurrentCulture).Contains(searchCriteria.SearchText.ToLower(CultureInfo.CurrentCulture)) ||
                        person.Email.ToLower(CultureInfo.CurrentCulture).Contains(searchCriteria.SearchText.ToLower(CultureInfo.CurrentCulture)) ||
                        person.EmployeeNumber.ToLower(CultureInfo.CurrentCulture).Contains(searchCriteria.SearchText.ToLower(CultureInfo.CurrentCulture)) ||
                        person.Note.ToLower(CultureInfo.CurrentCulture).Contains(searchCriteria.SearchText.ToLower(CultureInfo.CurrentCulture)) ||
                        person.CultureInfo.DisplayName.ToLower(CultureInfo.CurrentCulture).Contains(searchCriteria.SearchText.ToLower(CultureInfo.CurrentCulture)) ||
                        person.TimeZone.ToLower(CultureInfo.CurrentCulture).Contains(searchCriteria.SearchText.ToLower(CultureInfo.CurrentCulture)) ||
                        person.LogOnName.ToLower(CultureInfo.CurrentCulture).Contains(searchCriteria.SearchText.ToLower(CultureInfo.CurrentCulture)) ||
                        person.ApplicationLogOnName.ToLower(CultureInfo.CurrentCulture).Contains(searchCriteria.SearchText.ToLower(CultureInfo.CurrentCulture))

                    select person;
            }

            // REturns the search results
            return personQuery;
        }

        /// <summary>
        /// Fills the given search result data to the given grid control.
        /// </summary>
        /// <param name="grid">GRid control</param>
        /// <param name="searchResults">Search results</param>
        private static void FillGrid(GridControl grid, IEnumerable<PersonGeneralModel> searchResults)
        {
            // Sets the row index for the grid control
            int rowIndex = grid.RowCount + 1;

            foreach (PersonGeneralModel personEntity in searchResults)
            {
                // Sets the row count of the grid control
                grid.RowCount = rowIndex;

                // Fills the search result table
                grid[rowIndex, 1].Text = personEntity.FirstName + " " + personEntity.LastName;
                grid[rowIndex, 2].Text = string.Empty;
                grid[rowIndex, 3].Text = string.Empty;

                rowIndex++;
            }
        }

        #endregion

        #endregion
    }

}
