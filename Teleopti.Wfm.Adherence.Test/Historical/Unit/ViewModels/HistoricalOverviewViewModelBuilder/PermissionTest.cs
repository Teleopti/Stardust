using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalOverviewViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	[FakePermissions]
	public class PermissionTest : IIsolateSystem
	{
		public Adherence.ApplicationLayer.ViewModels.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePermissions FakePermissions;

		[Test]
		public void ShouldCalculateAdherenceWithoutPermissions()
		{
			FakePermissions.HasPermission(DefinedRaptorApplicationFunctionPaths.ModifySchedule);
			FakePermissions.HasPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules);

			Now.Is("2018-08-17 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-17")
				.WithAssignedActivity("2018-08-17 10:00", "2018-08-17 20:00")
				.WithHistoricalStateChange("2018-08-17 10:00", Domain.Configuration.Adherence.In)
				.WithHistoricalStateChange("2018-08-17 15:00", Domain.Configuration.Adherence.Out);
			Now.Is("2018-08-24 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Adherence.Should().Be("50");
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}
	}
}