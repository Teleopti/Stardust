using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.WebTest.Core.Portal
{
	[TestFixture]
	public class PortalHtmlHelperTest
	{
		[Test]
		public void ShouldGetFirstSectionAsDefaltPortalView()
		{
			const string expectedResult = "C1/A1";
			var portalViewModel = new PortalViewModel
			                      	{
			                      		NavigationItems = new List<NavigationItem>
			                      		                  	{
			                      		                  		new NavigationItem
			                      		                  			{
			                      		                  				Controller = "C1",
			                      		                  				Action = "A1",
			                      		                  				Title = "C1A1"
			                      		                  			},
			                      		                  		new NavigationItem
			                      		                  			{
			                      		                  				Controller = "C1",
			                      		                  				Action = "A2",
			                      		                  				Title = "C1A2"
			                      		                  			}
			                      		                  	}
			                      	};

			var target = new PortalHtmlHelper();

			var result = target.GetDefaultAction(portalViewModel);

			result.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldFormatNavigationItemAction()
		{
			const string expectedResult = "C2/A2";

			var navItem = new NavigationItem
			              	{
			              		Controller = "C2",
			              		Action = "A2",
			              		Title = "C2A2"
			              	};
			var target = new PortalHtmlHelper();

			var result = target.GetAction(navItem);

			result.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldFormatNavigationItemId()
		{
			const string expectedResult = "C2_A2";

			var navItem = new NavigationItem
			              	{
			              		Controller = "C2",
			              		Action = "A2",
			              		Title = "C2A2"
			              	};
			var target = new PortalHtmlHelper();

			var result = target.GetId(navItem);

			result.Should().Be.EqualTo(expectedResult);
		}

	}
}