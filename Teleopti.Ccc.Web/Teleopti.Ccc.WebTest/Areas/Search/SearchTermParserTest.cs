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
		[Ignore]
		public void ShouldParseSearchTermString()
		{
			var searchTerm = "FirstName: aa bb";

			var result = SearchTermParser.Parse(searchTerm);

			result.Count().Equals(1);
			result.First().Key.Equals(PersonFinderField.FirstName);
			result.First().Value.Equals("aa bb");
		}
	}
}
