using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	public class SearchTermParser : ISearchTermParser
	{
		private readonly ILoggedOnUser _loggonUser;
		private readonly IToggleManager _toggleManager;

		public SearchTermParser(ILoggedOnUser loggonUser, IToggleManager toggleManager)
		{
			_loggonUser = loggonUser;
			_toggleManager = toggleManager;
		}

		public static IDictionary<PersonFinderField, string> Parse(string values)
		{
			const char keyValueSplitter = ':';
			const char keywordsSplitter = ' ';
			const char searchTermSplitter = ';';

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

		public IDictionary<PersonFinderField, string> Parse(string values, DateOnly date)
		{
			if (!_toggleManager.IsEnabled(Toggles.WfmTeamSchedule_DisplayScheduleOnBusinessHierachy_41260))
				values = Keyword(values, date);
			return Parse(values);
		}

		public string Keyword(string values, DateOnly date)
		{
			var myTeam = _loggonUser.CurrentUser().MyTeam(date);

			if (string.IsNullOrEmpty(values))
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

	public interface ISearchTermParser
	{
		IDictionary<PersonFinderField, string> Parse(string values, DateOnly date);
		string Keyword(string values, DateOnly date);
	}
}