using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	public static class SearchTermParser
	{
		private static int _MAX_FIELD_VALUE_LENGTH = 500;
		public static IDictionary<PersonFinderField, string> Parse(string values)
		{
			const char keyValueSplitter = ':';
			const char keywordsSplitter = ' ';
			const char searchTermSplitter = ';';

			var parsedTerms = new Dictionary<PersonFinderField, string>();

			if (string.IsNullOrWhiteSpace(values))
			{
				return parsedTerms;
			}

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
				if (parsedTerms[searchType].Length > _MAX_FIELD_VALUE_LENGTH)
					return new Dictionary<PersonFinderField, string>();
			}

			return parsedTerms;
		}

		public static string KeywordWithDefault(string values, DateOnly date, ITeam myTeam)
		{
			if (string.IsNullOrEmpty(values) && myTeam != null)
			{
				var siteTerm = myTeam.Site.Description.Name.Contains(" ")
					? "\"" + myTeam.Site.Description.Name + "\""
					: myTeam.Site.Description.Name;
				var teamTerm = myTeam.Description.Name.Contains(" ")
					? "\"" + myTeam.Description.Name + "\""
					: myTeam.Description.Name;
				values = siteTerm + " " + teamTerm;
			}
			return values;
		}
	}
}