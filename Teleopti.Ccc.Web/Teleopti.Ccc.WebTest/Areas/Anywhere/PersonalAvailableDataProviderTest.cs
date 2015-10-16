using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class PersonalAvailableDataProviderTest
	{
		[Test]
		public void ShouldNotGetSitesWithoutPermission()
		{
			var site = new Site("NotPermitted").WithId();
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			siteRepository.Stub(x => x.LoadAll()).Return(new[] {site});
			principalAuthorization.Stub(
				x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2015-8-12".Date(), site))
				.Return(false);

			var target = new PersonalAvailableDataProvider(siteRepository, new PermissionProvider(principalAuthorization));
			var result = target.AvailableSites(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2015-8-12".Date());

			result.Should().Have.Count.EqualTo(0);
		}
		
		[Test]
		public void ShouldGetSitesWithPermission()
		{
			var site = new Site("Permitted").WithId();
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			siteRepository.Stub(x => x.LoadAll()).Return(new[] {site});
			principalAuthorization.Stub(
				x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2015-8-12".Date(), site))
				.Return(true);

			var target = new PersonalAvailableDataProvider(siteRepository, new PermissionProvider(principalAuthorization));
			var result = target.AvailableSites(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2015-8-12".Date());

			result.Single().Should().Be(site);
		}
	}
}
