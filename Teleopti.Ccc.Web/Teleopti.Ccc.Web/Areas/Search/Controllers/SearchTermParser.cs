using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	public static class SearchTermParser
	{
		public static IDictionary<PersonFinderField, string> Parse(string values)
		{
			const char keyValueSplitter = ':';
			const char keywordsSplitter = ' ';
			const char searchTermSplitter = ',';

			var parsedTerms = new Dictionary<PersonFinderField, string>();
			if (!values.Contains(keyValueSplitter))
			{
				parsedTerms.Add(PersonFinderField.All, values);
				return parsedTerms;
			}

			var quotePatternRegex = new Regex("\\s*(\"[^\"]*?\")\\s*");
			var splitPattern =
				new Regex("(?<!\"[^" + keywordsSplitter + "]+)" + keywordsSplitter + "(?![^" + keywordsSplitter + "]+\")");

			values = Regex.Replace(values, @"\s{1,}", " ");
			var searchTerms = values.Split(searchTermSplitter)
				.Select(s => s.Trim())
				.Where(s => !string.IsNullOrEmpty(s));
			foreach (var term in searchTerms)
			{
				var splitterPosition = term.IndexOf(keyValueSplitter);
				if (splitterPosition < 0) continue;

				var searchTypeString = term.Substring(0, splitterPosition).Trim();

				PersonFinderField searchType;
				if (!Enum.TryParse(searchTypeString, true, out searchType))
				{
					continue;
				}

				var searchValues = term.Substring(splitterPosition + 1, term.Length - splitterPosition - 1).Trim();

				// Replace multiple spaces and tabs before/after quote pair to single space
				// Or add space before/after quote pair if there is no space
				searchValues = quotePatternRegex.Replace(searchValues, " $1 ");

				// Split search value into single keyword based on space and quote pair
				var result = splitPattern.Split(searchValues)
					.Select(x => x.Trim())
					.Where(x => !string.IsNullOrEmpty(x));

				var searchKeywords = parsedTerms.ContainsKey(searchType)
					? parsedTerms[searchType].Split(keywordsSplitter).ToList()
					: new List<string>();

				searchKeywords.AddRange(result);
				parsedTerms[searchType] = string.Join(keywordsSplitter.ToString(CultureInfo.CurrentCulture),
					new HashSet<string>(searchKeywords));
			}

			return parsedTerms;
		}

		public static IDictionary<PersonFinderField, string> Parse(PeopleSearchCriteria form)
		{
			var criterias = new Dictionary<PersonFinderField, string>();

			addSearchCriteria(criterias, PersonFinderField.FirstName, form.FirstName);
			addSearchCriteria(criterias, PersonFinderField.LastName, form.LastName);
			addSearchCriteria(criterias, PersonFinderField.BudgetGroup, form.BudgetGroup);
			addSearchCriteria(criterias, PersonFinderField.Contract, form.Contract);
			addSearchCriteria(criterias, PersonFinderField.ContractSchedule, form.ContractSchedule);
			addSearchCriteria(criterias, PersonFinderField.EmploymentNumber, form.EmploymentNumber);
			addSearchCriteria(criterias, PersonFinderField.Note, form.Note);
			addSearchCriteria(criterias, PersonFinderField.Organization, form.Organization);
			addSearchCriteria(criterias, PersonFinderField.PartTimePercentage, form.PartTimePercentage);
			addSearchCriteria(criterias, PersonFinderField.Role, form.Role);
			addSearchCriteria(criterias, PersonFinderField.ShiftBag, form.ShiftBag);
			addSearchCriteria(criterias, PersonFinderField.Skill, form.Skill);

			return criterias;
		}

		private static void addSearchCriteria(IDictionary<PersonFinderField, string> criterias,
			PersonFinderField searchType, string searchValue)
		{
			if (string.IsNullOrEmpty(searchValue)) return;

			criterias.Add(searchType, searchValue);
		}
	}
}