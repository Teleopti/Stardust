using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Search.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	[TestFixture]
	public class SearchTermParserTest
	{
		[Test]
		public void ShouldParseEmptySearchTermString()
		{
			const string searchTerm = "  ";
			var result = SearchTermParser.Parse(searchTerm);

			Assert.AreEqual(0,result.Count);
		
		}


		[Test]
		public void ShouldParseSearchTermString()
		{
			const string searchTerm = "FirstName: aa bb; LastName: cc dd";
			var result = SearchTermParser.Parse(searchTerm);

			Assert.AreEqual(2, result.Count);

			var firstTerm = result.First();
			Assert.AreEqual(PersonFinderField.FirstName, firstTerm.Key);
			Assert.AreEqual("aa bb", firstTerm.Value);

			var secondTerm = result.Second();
			Assert.AreEqual(PersonFinderField.LastName, secondTerm.Key);
			Assert.AreEqual("cc dd", secondTerm.Value);
		}

		[Test]
		public void ShouldHandleMultipleSpacesAndTabs()
		{
			const string searchTerm = "FirstName: \taa    bb; \t LastName  \t  :   cc   \tdd  ";
			var result = SearchTermParser.Parse(searchTerm);

			Assert.AreEqual(2, result.Count);

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
			const string searchTerm = "Firstme: aa bb; lastName: cc dd";
			var result = SearchTermParser.Parse(searchTerm);

			Assert.AreEqual(1, result.Count);

			var firstTerm = result.First();
			Assert.AreEqual(PersonFinderField.LastName, firstTerm.Key);
			Assert.AreEqual("cc dd", firstTerm.Value);
		}

		[Test]
		public void ShouldParseStringWithoutType()
		{
			const string searchTerm = "aa bb cc dd";
			var result = SearchTermParser.Parse(searchTerm);

			Assert.AreEqual(1, result.Count);

			var firstTerm = result.First();
			Assert.AreEqual(PersonFinderField.All, firstTerm.Key);
			Assert.AreEqual("aa bb cc dd", firstTerm.Value);
		}

		[Test]
		public void ShouldHandleDuplicateSearchType()
		{
			const string searchTerm = "Firstname: aa bb; Firstname: ee";
			var result = SearchTermParser.Parse(searchTerm);

			Assert.AreEqual(1, result.Count);

			var firstTerm = result.First();
			Assert.AreEqual(PersonFinderField.FirstName, firstTerm.Key);
			Assert.AreEqual("aa bb ee", firstTerm.Value);
		}

		[Test]
		public void ShouldParseSearchTermStringWithAdvancedSingleSearchTerm()
		{
			const string advancedSingleSearchTerm = "FirstName:aa bb";
			var result = SearchTermParser.Parse(advancedSingleSearchTerm);

			Assert.AreEqual(1, result.Count);

			var firstTerm = result.First();
			Assert.AreEqual(PersonFinderField.FirstName, firstTerm.Key);
			Assert.AreEqual("aa bb", firstTerm.Value);
		}

		[Test]
		public void ShouldRemoveDuplicateKeywordForDuplicateSearchType()
		{
			const string searchTerm = "Firstname: aa bb; Firstname: aa ee";
			var result = SearchTermParser.Parse(searchTerm);

			Assert.AreEqual(1, result.Count);

			var firstTerm = result.First();
			Assert.AreEqual(PersonFinderField.FirstName, firstTerm.Key);
			Assert.AreEqual("aa bb ee", firstTerm.Value);
		}

		[Test]
		public void ShouldHandleSearchKeywordWithQuote()
		{
			const string searchTerm = "Firstname: \"aa bb\"dd; Firstname: aa ee";
			var result = SearchTermParser.Parse(searchTerm);

			Assert.AreEqual(1, result.Count);

			var firstTerm = result.First();
			Assert.AreEqual(PersonFinderField.FirstName, firstTerm.Key);
			Assert.AreEqual("\"aa bb\" dd aa ee", firstTerm.Value);
		}

		[Test]
		public void ShouldIgnoreInvalidTerm()
		{
			const string invalidInput = "FirstName: aa; bb";
			var result = SearchTermParser.Parse(invalidInput);
			Assert.AreEqual(result.Count, 1);

			var firstTerm = result.First();
			Assert.AreEqual("aa", firstTerm.Value);
		}

		[Test]
		public void ShouldNotParseAnyTermIfOneFieldValueLongerThan500()
		{
			const string invalidInput = "All: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaab";
			var result = SearchTermParser.Parse(invalidInput);
			Assert.AreEqual(result.Count, 0);
		}
	}
}
