using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class LinkProviderTest
	{
		[Test]
		public void ShouldProvideRequestDetailLink()
		{
			var urlHelperBuilder = new TestUrlHelperBuilder();
			urlHelperBuilder.Routes(new TestRouteBuilder().MakeAreaDefaultRoute("MyTime"));
			var urlHelper = MockRepository.GenerateMock<IUrlHelper>();
			urlHelper.Expect(x => x.Fetch()).Return(urlHelperBuilder.MakeUrlHelper("http://hostname/MyTime/Controller/Action"));
			var target = new LinkProvider(urlHelper);
			var id = Guid.NewGuid();

			var result = target.RequestDetailLink(id);

			result.Should().Be("/MyTime/Requests/RequestDetail/" + id);
		}
	}
}
