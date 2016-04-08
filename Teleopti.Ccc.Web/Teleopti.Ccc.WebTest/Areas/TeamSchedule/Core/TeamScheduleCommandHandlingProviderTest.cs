using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core
{
	[TestFixture, TeamScheduleTest]
	public class TeamScheduleCommandHandlingProviderTest
	{
		public ITeamScheduleCommandHandlingProvider Target;
		public FakeActivityCommandHandler AddActivityCommandHandler;
		public FakePersonRepository PersonRepository;
		public PrincipalAuthorizationWithConfigurablePermission PrincipalAuthorizationWithConfigurablePermission;

		[Test]
		public void ShouldInvokeAddActivityCommandHandleWithCorrectCommandData()
		{
			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				BelongsToDate = DateOnly.Today,
				StartTime = new DateTime(2016,3,28, 8, 0, 0),
				EndTime = new DateTime(2016, 3, 28, 17, 0, 0),
				PersonIds = new [] { Guid.NewGuid(),Guid.NewGuid() },
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			Target.AddActivity(input);

			AddActivityCommandHandler.CalledCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnNoWriteProtectedAgentsIfHasModifyWriteProtectedSchedulePermission()
		{
			PrincipalAuthorization.SetInstance(PrincipalAuthorizationWithConfigurablePermission);
			PrincipalAuthorizationWithConfigurablePermission.HasPermission(DefinedRaptorApplicationFunctionPaths.SetWriteProtection);
			PrincipalAuthorizationWithConfigurablePermission.HasPermission(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);

			var agenta = PersonFactory.CreatePersonWithGuid("a", "a");
			var agentb = PersonFactory.CreatePersonWithGuid("b", "b");


			PersonRepository.Has(agenta);
			PersonRepository.Has(agentb);

			agenta.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 1, 1);
			agentb.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 5, 1);

			var result = Target.CheckWriteProtectedAgents(new DateOnly(2016, 3, 1), new[] {agenta.Id.Value, agentb.Id.Value});
			result.ToList().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnWriteProtectedAgentsIfWithoutModifyWriteProtectedSchedulePermission()
		{
			PrincipalAuthorization.SetInstance(PrincipalAuthorizationWithConfigurablePermission);
			PrincipalAuthorizationWithConfigurablePermission.HasPermission(DefinedRaptorApplicationFunctionPaths.SetWriteProtection);
			
			var agenta = PersonFactory.CreatePersonWithGuid("a","a");
			var agentb = PersonFactory.CreatePersonWithGuid("b","b");

			PersonRepository.Has(agenta);
			PersonRepository.Has(agentb);

			agenta.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016,1,1);
			agentb.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016,5,1);

			var result = Target.CheckWriteProtectedAgents(new DateOnly(2016,3,1),new[] { agenta.Id.Value,agentb.Id.Value });
			result.ToList().Count.Should().Be.EqualTo(1);
		}
	}

	public class FakeActivityCommandHandler : IHandleCommand<AddActivityCommand>
	{
		private int calledCount;
		public void Handle(AddActivityCommand command)
		{
			calledCount++;
		}

		public int CalledCount
		{
			get { return calledCount; }
		}

		public void ResetCalledCount()
		{
			calledCount = 0;
		}
	}
}
