using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	[RealPermissions]
	public class AgentsInAlarmForPermittedSiteViewModelBuilderTest //: ISetup
	{
		public SiteInAlarmViewModelBuilder Target;
		public FakeDatabase Database;
		public FakePersonRepository Persons;
		public ILogOnOff LogOnOff;

		[Test, Ignore("WIP")]
		public void ShouldBuildForPermittedSiteOnlyForTeamLevel()
		{
			var meId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
	
			Database
				.WithTenant("tenant")
				.WithSite(siteId)

				.WithTeam("my team")
				.WithAgent(meId, "me")
				.WithRole(AvailableDataRangeOption.MyTeam, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)
				;
			var me = Persons.Load(meId);
			
			LogOnOff.LogOn("tenant", me, Database.CurrentBusinessUnitId());


			var viewModel = Target.Build().Single();

			viewModel.Id.Should().Be(siteId);


		}

	}
}