using System;
using System.Collections.Generic;
using System.Linq;
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
			var searchTerms = values.Split(new[] {','}).Select(s => s.Trim()).Where(s=>!string.IsNullOrEmpty(s));
			foreach (var term in searchTerms)
			{
				var splitter = term.IndexOf(':');
				var type = term.Substring(0, splitter).Trim();
				var value = term.Substring(splitter + 1, term.Length - splitter - 1).Trim();
				PersonFinderField parsedType;
				if (Enum.TryParse(type, true, out parsedType))
				{
					parsedTerms.Add(parsedType, value);
				}
			}
			
			return parsedTerms;
		}

		public static IDictionary<PersonFinderField, string> Parse(PeopleSearchCriteria form)
		{
			return null;
		}
	}
}