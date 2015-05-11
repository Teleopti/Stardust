using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	public static class SearchTermParser
	{
		public static IDictionary<PersonFinderField, string> Parse(string values)
		{

			//var index = values.IndexOf(':');
			//var searchTypeLength = values.Length - index;
			//var searchType = values.Substring(0, searchTypeLength);
			//var searchValue = values.Substring(searchTypeLength - 1, values.Length - searchType.Length);
			//IDictionary<PersonFinderField, string> parsedString = new Dictionary<PersonFinderField, string>();


			//parsedString.Add((PersonFinderField)Enum.Parse(typeof(PersonFinderField), searchType), searchValue);
			//return parsedString;
			return null;
		}

		public static IDictionary<PersonFinderField, string> Parse(PeopleSearchCriteria form)
		{
			return null;
		}
	}
}