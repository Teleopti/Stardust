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

			Assert.AreEqual(2, result.Count());

			var firstTerm = result.First();
			Assert.AreEqual(PersonFinderField.FirstName, firstTerm.Key);
			Assert.AreEqual("aa bb", firstTerm.Value);

			var secondTerm = result.Second();
			Assert.AreEqual(PersonFinderField.LastName, secondTerm.Key);
			Assert.AreEqual("cc dd", secondTerm.Value);
		}

		[Test]
		public void ShouldParseSearchTermStringWithInvalidType()
		{
			const string searchTerm = "Firstme: aa bb, lastName: cc dd";
			var result = SearchTermParser.Parse(searchTerm);

			Assert.AreEqual(1, result.Count());

			var firstTerm = result.First();
			Assert.AreEqual(PersonFinderField.LastName, firstTerm.Key);
			Assert.AreEqual("cc dd", firstTerm.Value);
		}

		[Test]
		public void ShouldParseStringWithoutType()
		{
			const string searchTerm = "aa bb cc dd";
			var result = SearchTermParser.Parse(searchTerm);

			Assert.AreEqual(1, result.Count());

			var firstTerm = result.First();
			Assert.AreEqual(PersonFinderField.All, firstTerm.Key);
			Assert.AreEqual("aa bb cc dd", firstTerm.Value);
		}

		[Test]
		public void ShouldHandleDuplicateSearchType()
		{
			const string searchTerm = "Firstname: aa bb, Firstname: ee";
			var result = SearchTermParser.Parse(searchTerm);

			Assert.AreEqual(1, result.Count());

			var firstTerm = result.First();
			Assert.AreEqual(PersonFinderField.FirstName, firstTerm.Key);
			Assert.AreEqual(firstTerm.Value, "aa bb ee");
		}

		[Test]
		public void ShouldParseSearchTermStringWithAdvancedSingleSearchTerm()
		{
			const string advancedSingleSearchTerm = "   FirstName  :   aa	   bb   ";
			var advancedSingleSearchTermResult = SearchTermParser.Parse(advancedSingleSearchTerm);
			Assert.AreEqual(1, advancedSingleSearchTermResult.Count());
			Assert.AreEqual(PersonFinderField.FirstName, advancedSingleSearchTermResult.First().Key);
			Assert.AreEqual("aa bb", advancedSingleSearchTermResult.First().Value);
		}

		[Test]
		public void ShouldParseSearchCriterias()
		{
			var criteria = new PeopleSearchCriteria {FirstName = "a b", Organization = "London"};

			var result = SearchTermParser.Parse(criteria);

			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(PersonFinderField.FirstName, result.First().Key);
			Assert.AreEqual("a b", result.First().Value);
			Assert.AreEqual(PersonFinderField.Organization, result.Second().Key);
			Assert.AreEqual("London", result.Second().Value);
		}
		
		[Test]
		public void ShouldAlertUserForInvalidInput()
		{
			const string invalidInput = "FirstName: aa, bb";
			Assert.Catch(() => SearchTermParser.Parse(invalidInput));
		}
	}
}
