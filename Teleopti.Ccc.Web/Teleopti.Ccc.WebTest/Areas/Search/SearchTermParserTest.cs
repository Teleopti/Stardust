using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	[TestFixture]
	public class SearchTermParserTest
	{
		[Test]
		public void ShouldParseSearchTermString()
		{
			const string searchTerm = "FirstName: aa bb, LastName: cc dd";
			var result = SearchTermParser.Parse(searchTerm);

			result.Count().Equals(2);

			var firstTerm = result.First();
			firstTerm.Key.Equals(PersonFinderField.FirstName);
			firstTerm.Value.Equals("aa bb");

			var secondTerm = result.Second();
			secondTerm.Key.Equals(PersonFinderField.LastName);
			secondTerm.Value.Equals("cc dd");
		}

		[Test]
		public void ShouldParseSearchTermStringWithInvalidType()
		{
			const string searchTerm = "Firstme: aa bb, lastName: cc dd";
			var result = SearchTermParser.Parse(searchTerm);

			result.Count().Equals(1);

			var firstTerm = result.First();
			firstTerm.Key.Equals(PersonFinderField.LastName);
			firstTerm.Value.Equals("cc dd");
		}

		[Test]
		public void ShouldParseStringWithoutType()
		{
			const string searchTerm = "aa bb cc dd";
			var result = SearchTermParser.Parse(searchTerm);

			result.Count().Equals(1);

			var firstTerm = result.First();
			firstTerm.Key.Equals(PersonFinderField.All);
			firstTerm.Value.Equals("aa bb cc dd");
		}

		[Test]
		public void ShouldHandleDuplicateSearchType()
		{
			const string searchTerm = "Firstname: aa bb, lastName: cc dd, Firstname: ee";
			var result = SearchTermParser.Parse(searchTerm);

			result.Count().Equals(1);

			var firstTerm = result.First();
			firstTerm.Key.Equals(PersonFinderField.FirstName);
			firstTerm.Value.Equals("aa bb ee");
		}
	}
}
