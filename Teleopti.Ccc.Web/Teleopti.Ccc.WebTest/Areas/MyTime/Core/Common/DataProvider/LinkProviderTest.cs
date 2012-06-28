using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class LinkProviderTest
	{
		[Test]
		public void ShouldProvideTextRequestLink()
		{
			var urlHelperBuilder = new TestUrlHelperBuilder();
			urlHelperBuilder.Routes(new TestRouteBuilder().MakeAreaDefaultRoute("MyTime"));
			var urlHelper = urlHelperBuilder.MakeUrlHelper("http://hostname/MyTime/Controller/Action");
			var target = new LinkProvider(urlHelper.Resolver());
			var id = Guid.NewGuid();

			var result = target.RequestDetailLink(id);

			result.Should().Be("http://hostname/MyTime/Requests/RequestDetail/" + id);
		}
	}
}
