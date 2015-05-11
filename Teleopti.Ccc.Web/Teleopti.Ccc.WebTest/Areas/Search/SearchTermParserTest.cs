using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			var searchTerm = "FirstName: aa bb, LastName: cc dd";

			var result = SearchTermParser.Parse(searchTerm);

			result.Count().Equals(2);
			result.First().Key.Equals(PersonFinderField.FirstName);
			result.Second().Key.Equals(PersonFinderField.LastName);
			result.First().Value.Equals("aa bb");
			result.Second().Value.Equals("cc dd");
		}

		[Test]
		public void ShouldParseSearchTermStringWithInvalidType()
		{
			var searchTerm = "Firstme: aa bb, lastName: cc dd";

			var result = SearchTermParser.Parse(searchTerm);

			result.Count().Equals(1);
			result.First().Key.Equals(PersonFinderField.LastName);
			result.First().Value.Equals("cc dd");
		}

		[Test]
		public void ShouldParseStringWithoutType()
		{
			var searchTerm = "aa bb cc dd";

			var result = SearchTermParser.Parse(searchTerm);

			result.Count().Equals(1);
			result.First().Key.Equals(PersonFinderField.All);
			result.First().Value.Equals("aa bb cc dd");
		}
	}
}
