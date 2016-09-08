using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class SiteProviderTest
	{
		[Test]
		public void ShouldFilterPermittedTeamsWhenQueryingAll()
		{
			var repository = MockRepository.GenerateMock<ISiteRepository>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var sites = new ISite[] { new Site("site1"), new Site("site2"),  };

			repository.Stub(x => x.LoadAllOrderByName()).Return(sites);
			permissionProvider.Stub(x => x.HasSitePermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, sites.ElementAt(0))).Return(false);
			permissionProvider.Stub(x => x.HasSitePermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, sites.ElementAt(1))).Return(true);

			var target = new SiteProvider(repository, permissionProvider);

			var result = target.GetPermittedSites(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			result.Single().Should().Be(sites.ElementAt(1));
		}
	}
}
