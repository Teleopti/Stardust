using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
	public class PeoplePeriodDomainFinder : IDomainFinder
	{
        private readonly FilteredPeopleHolder _filteredPeopleHolder;

		public PeoplePeriodDomainFinder(FilteredPeopleHolder filteredPeopleHolder)
        {
            _filteredPeopleHolder = filteredPeopleHolder;
        }

		/// <summary>
		/// Finds the people data that meets the search criteria given,
		///  and fills the search results to the given grid
		/// </summary>
		/// <param name="grid">Grid to display the search data</param>
		/// <param name="searchCriteria">Search criteria</param>
        public void Find(GridControl grid, SearchCriteria searchCriteria)
        {
            IList<IPerson> collection = _filteredPeopleHolder.FilteredPersonCollection;
            IList<IPerson> sortedList = (from person in collection
                                         orderby person.Name.ToString() ascending
                                         select (IPerson)person).ToList();

            for (int index = 0; index < sortedList.Count; index++)
            {
                IPerson person = sortedList[index];
	            bool isExistsInFilteredList = collection.Contains(person);

	            if (isExistsInFilteredList)
                {
                    bool isItemFound = Find(searchCriteria, person);

                    if (!isItemFound)
                    {
                        IList<IPersonPeriod> wrapper = new List<IPersonPeriod>(person.PersonPeriodCollection);
                        for (int periodIndex = 0; periodIndex < wrapper.Count; periodIndex++)
                        {
                            IPersonPeriod personPeriod = wrapper[periodIndex];
                            isItemFound = Find(searchCriteria, personPeriod);

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

        private static bool Find(SearchCriteria searchCriteria, IPersonPeriod personPeriod)
        {
            bool isFound;
            string searchText = searchCriteria.SearchText;
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            if (searchCriteria.IsCaseSensitive)
            {
                isFound = personPeriod.StartDate.ToString().Contains(searchText) ||
                          personPeriod.Team.Description.Name.Contains((searchText)) ||
                          personPeriod.PersonContract.Contract.Description.Name.Contains(searchText) ||
                          GetSkills(personPeriod.PersonSkillCollection).Contains(searchText) ||
                          personPeriod.PersonContract.ContractSchedule.Description.Name.Contains(searchText) ||
                          personPeriod.PersonContract.PartTimePercentage.Description.Name.Contains(searchText) ||
                          GetRulSetBag(personPeriod.RuleSetBag).Contains(searchText) ||
                          GetPersonExternalLogOnNames(personPeriod.ExternalLogOnCollection).Contains(searchText) ||
                          personPeriod.Note.Contains(searchText);
            }
            else
            {
                searchText = searchText.ToLower(currentCulture);
                isFound =
                    personPeriod.StartDate.ToString().ToLower(currentCulture).Contains(searchText) ||
                    personPeriod.Team.Description.Name.ToLower(currentCulture).Contains((searchText)) ||
                    personPeriod.PersonContract.Contract.Description.Name.ToLower(currentCulture).Contains(searchText) ||
                    GetSkills(personPeriod.PersonSkillCollection).ToLower(currentCulture).Contains(searchText) ||
                    personPeriod.PersonContract.ContractSchedule.Description.Name.ToLower(currentCulture).Contains(
                        searchText) ||
                    personPeriod.PersonContract.PartTimePercentage.Description.Name.ToLower(currentCulture).Contains(
                        searchText) ||
                    GetRulSetBag(personPeriod.RuleSetBag).ToLower(currentCulture).Contains(searchText) ||
                    GetPersonExternalLogOnNames(personPeriod.ExternalLogOnCollection).ToLower(currentCulture).Contains(
                        searchText) ||
                    personPeriod.Note.ToLower(currentCulture).Contains(searchText);
            }

            return isFound;
        }

        private static string GetSkills(IEnumerable<IPersonSkill> personSkillCollection)
        {
            StringBuilder personSkillString = new StringBuilder();

            if (personSkillCollection != null)
            {
                IList<IPersonSkill> skillCollection = personSkillCollection.OrderBy(s => s.Skill.Name).ToList();

                foreach (IPersonSkill personSkill in skillCollection)
                {
                    if (!string.IsNullOrEmpty(personSkillString.ToString()))
                        personSkillString.Append(", " + personSkill.Skill.Name);
                    else personSkillString.Append(personSkill.Skill.Name);
                }
            }
            return personSkillString.ToString();
        }

        private static string GetPersonExternalLogOnNames(IEnumerable<IExternalLogOn> personExternalLogOnCollection)
        {
            StringBuilder personExternalLogOnNameString = new StringBuilder();

            if (personExternalLogOnCollection != null)
            {
                IList<IExternalLogOn> externalLogOnCollection = 
                    personExternalLogOnCollection.OrderBy(s => s.AcdLogOnName).ToList();

                foreach (IExternalLogOn externalLogOn in externalLogOnCollection)
                {
                    if (!string.IsNullOrEmpty(personExternalLogOnNameString.ToString()))
                        personExternalLogOnNameString.Append(", " + externalLogOn.AcdLogOnName);
                    else personExternalLogOnNameString.Append(externalLogOn.AcdLogOnName);
                }

                return personExternalLogOnNameString.ToString();
            }

            return string.Empty;
        }

        private static string GetRulSetBag(IRuleSetBag ruleSetBag)
        {
            string ruleSetString = string.Empty;

            if (ruleSetBag != null)
            {
                ruleSetString = ruleSetBag.Description.Name;
            }

            return ruleSetString;
        }
	}
}
