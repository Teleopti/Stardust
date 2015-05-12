using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	public static class SearchTermParser
	{
		public static IDictionary<PersonFinderField, string> Parse(string values)
		{
			var parsedTerms = new Dictionary<PersonFinderField, string>();
			if (!values.Contains(":"))
			{
				parsedTerms.Add(PersonFinderField.All, values);
				return parsedTerms;
			}

			var quotePatternRegex = new Regex("\\s*(\"[^\"]*?\")\\s*");
			var splitPattern = new Regex("(?<!\"[^ ]+) (?![^ ]+\")");

			values = Regex.Replace(values, @"\s{1,}", " ");
			var searchTerms = values.Split(new[] {','}).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
			foreach (var term in searchTerms)
			{
				var splitterPosition = term.IndexOf(':');
				if (splitterPosition < 0) continue;

				var searchTypeString = term.Substring(0, splitterPosition).Trim();

				PersonFinderField searchType;
				if (!Enum.TryParse(searchTypeString, true, out searchType))
				{
					continue;
				}

				var searchValues = term.Substring(splitterPosition + 1, term.Length - splitterPosition - 1).Trim();

				// Replace multiple spaces and tabs before/after quote pair to single space
				searchValues = quotePatternRegex.Replace(searchValues, " $1 ");

				// Split search value into single keyword based on space and quote pair
				var result = splitPattern.Split(searchValues)
					.Select(x => x.Trim())
					.Where(x => !string.IsNullOrEmpty(x));

				var searchKeywords = parsedTerms.ContainsKey(searchType)
					? parsedTerms[searchType].Split(new[] {' '}).ToList()
					: new List<string>();

				searchKeywords.AddRange(result);
				parsedTerms[searchType] = string.Join(" ", new HashSet<string>(searchKeywords));
			}

			return parsedTerms;
		}

		public static IDictionary<PersonFinderField, string> Parse(PeopleSearchCriteria form)
		{
			var criteriaDictionary = new Dictionary<PersonFinderField, string>();
			if (!string.IsNullOrEmpty(form.FirstName))
			{
				criteriaDictionary.Add(PersonFinderField.FirstName, form.FirstName);
			}
			if (!string.IsNullOrEmpty(form.LastName))
			{
				criteriaDictionary.Add(PersonFinderField.LastName, form.LastName);
			}
			if (!string.IsNullOrEmpty(form.BudgetGroup))
			{
				criteriaDictionary.Add(PersonFinderField.BudgetGroup, form.BudgetGroup);
			}
			if (!string.IsNullOrEmpty(form.Contract))
			{
				criteriaDictionary.Add(PersonFinderField.Contract, form.Contract);
			}
			if (!string.IsNullOrEmpty(form.ContractSchedule))
			{
				criteriaDictionary.Add(PersonFinderField.ContractSchedule, form.ContractSchedule);
			}
			if (!string.IsNullOrEmpty(form.EmploymentNumber))
			{
				criteriaDictionary.Add(PersonFinderField.EmploymentNumber, form.EmploymentNumber);
			}
			if (!string.IsNullOrEmpty(form.Note))
			{
				criteriaDictionary.Add(PersonFinderField.Note, form.Note);
			}
			if (!string.IsNullOrEmpty(form.Organization))
			{
				criteriaDictionary.Add(PersonFinderField.Organization, form.Organization);
			}
			if (!string.IsNullOrEmpty(form.PartTimePercentage))
			{
				criteriaDictionary.Add(PersonFinderField.PartTimePercentage, form.PartTimePercentage);
			}
			if (!string.IsNullOrEmpty(form.Role))
			{
				criteriaDictionary.Add(PersonFinderField.Role, form.Role);
			}
			if (!string.IsNullOrEmpty(form.ShiftBag))
			{
				criteriaDictionary.Add(PersonFinderField.ShiftBag, form.ShiftBag);
			}
			if (!string.IsNullOrEmpty(form.Skill))
			{
				criteriaDictionary.Add(PersonFinderField.Skill, form.Skill);
			}
			return criteriaDictionary;
		}
	}
}