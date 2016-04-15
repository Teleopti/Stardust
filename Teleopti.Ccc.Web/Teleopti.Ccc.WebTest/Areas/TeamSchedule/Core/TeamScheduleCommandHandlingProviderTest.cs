using System;
using System.Collections.Generic;
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
		public FakeActivityCommandHandler ActivityCommandHandler;
		public FakePersonRepository PersonRepository;
		public PrincipalAuthorizationWithConfigurablePermission PrincipalAuthorizationWithConfigurablePermission;
		public FakePermissionProvider PermissionProvider;

		[Test]
		public void ShouldInvokeAddActivityCommandHandleWithPermission()
		{
			var person1 = PersonFactory.CreatePersonWithGuid("a","b");
			var person2 = PersonFactory.CreatePersonWithGuid("c","d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016,4,16);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddActivity,date,person1);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddActivity,date,person2);

			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				BelongsToDate = date,
				StartTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndTime = new DateTime(2016, 4, 16, 17, 0, 0),
				PersonIds = new [] { person1.Id.Value, person2.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.AddActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotInvokeAddActivityCommandHandleWithoutPermission()
		{
			var person1 = PersonFactory.CreatePersonWithGuid("a","b");
			var person2 = PersonFactory.CreatePersonWithGuid("c","d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016,4,16);

			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				BelongsToDate = date,
				StartTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndTime = new DateTime(2016, 4, 16, 17, 0, 0),
				PersonIds = new[] { person1.Id.Value, person2.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.AddActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotRemoveActivityWithoutPermission()
		{
			var person = PersonFactory.CreatePersonWithGuid("a","b");
			PersonRepository.Has(person);

			var date = new DateOnly(2016,4,16);
			var input = new RemoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<RemovePersonActivityItem>
				{
					new RemovePersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {new Guid(), new Guid()}
					},
					new RemovePersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {new Guid()}
					}
				},
				Date = date
			};

			ActivityCommandHandler.ResetCalledCount();
			Target.RemoveActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveActivityWithPermission()
		{
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);

			var date = new DateOnly(2016, 4, 16);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.RemoveActivity,date, person);

			var input = new RemoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<RemovePersonActivityItem>
				{
					new RemovePersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {new Guid(), new Guid()}
					},
					new RemovePersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {new Guid()}
					}
				},
				Date = date
			};

			ActivityCommandHandler.ResetCalledCount();
			Target.RemoveActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(3);
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

	public class FakeActivityCommandHandler : IHandleCommand<AddActivityCommand>, IHandleCommand<RemoveActivityCommand>
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

		public void Handle(RemoveActivityCommand command)
		{
			calledCount++;
		}
	}
}
