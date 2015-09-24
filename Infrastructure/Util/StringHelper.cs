using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class StringHelper
	{
		/// <summary>
		/// To split combined string list to a list.
		/// For example, "Agent, Administrator, "London, Team Leader"" will be splitted into a list with 3 string.
		/// </summary>
		/// <param name="combinedList">The string to be splitted</param>
		/// <param name="splitter">The splitter character</param>
		/// <param name="uniqueResult">Make result unique</param>
		/// <returns></returns>
		public static IEnumerable<string> SplitStringList(string combinedList, char splitter = ',', bool uniqueResult = true)
		{
			const string splitPatternTemplate = "[^{0}\"]+|\"[^\"]*\"";

			var splitPattern = string.Format(splitPatternTemplate, splitter);
			var matches = Regex.Matches(combinedList, splitPattern);

			var result =
				(from object match in matches select match.ToString().Trim('"', ' '))
					.Where(s => !string.IsNullOrEmpty(s));

			return uniqueResult ? new HashSet<string>(result) : result;
		}
	}
}
