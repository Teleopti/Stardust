using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.ViewModelFactory
{
	[TestFixture]
	public class SiteViewModelFactoryTest
	{
		[Test]
		public void ShouldCreateSiteOptionsViewModelForShiftTrade()
		{
			var site = new Site("mySite");
			site.SetBusinessUnit(new BusinessUnit("BU"));
			var sites = new[] { site };
			sites[0].SetId(Guid.NewGuid());
			var siteProvider = MockRepository.GenerateMock<ISiteProvider>();

			siteProvider.Stub(
				x => x.GetPermittedSites(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)).Return(sites);

			var target = new SiteViewModelFactory(siteProvider);

			var result = target.CreateSiteOptionsViewModel(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);

			result.Select(t => t.text).Should().Have.SameSequenceAs("BU/mySite");
		}
	}
}
