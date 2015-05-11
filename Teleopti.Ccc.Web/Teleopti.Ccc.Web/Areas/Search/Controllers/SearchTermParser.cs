using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Collection;
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
			values = Regex.Replace(values, @"\s+", " ");
			var searchTerms = values.Split(new[] {','}).Select(s => s.Trim()).Where(s=>!string.IsNullOrEmpty(s));
			foreach (var term in searchTerms)
			{
				var splitter = term.IndexOf(':');
				var type = term.Substring(0, splitter).Trim();
				var value = term.Substring(splitter + 1, term.Length - splitter - 1).Trim();
				
				PersonFinderField parsedType;
				if (!Enum.TryParse(type, true, out parsedType))
				{
					continue;
				}

				if (!parsedTerms.ContainsKey(parsedType))
				{
					parsedTerms.Add(parsedType, value);
				}
				else
				{
					parsedTerms[parsedType] = parsedTerms[parsedType] + " " + value;
				}
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