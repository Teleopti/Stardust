using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
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
		public Global.FakePermissionProvider PermissionProvider;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentRepo;
		public FakeCurrentScenario CurrentScenario;
		public FakeLoggedOnUser LoggedOnUser;

		[Test]
		public void ShouldInvokeAddActivityCommandHandleWithPermission()
		{
			var person1 = PersonFactory.CreatePersonWithGuid("a","b");
			var person2 = PersonFactory.CreatePersonWithGuid("c","d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016,4,16);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddActivity,person1, date);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddActivity,person2, date);

			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				Date = date,
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
			PermissionProvider.Enable();
			var person1 = PersonFactory.CreatePersonWithGuid("a","b");
			var person2 = PersonFactory.CreatePersonWithGuid("c","d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016,4,16);

			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				Date = date,
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
		public void ShouldInvokeAddPersonalActivityCommandHandleWithPermission()
		{
			var person1 = PersonFactory.CreatePersonWithGuid("a","b");
			var person2 = PersonFactory.CreatePersonWithGuid("c","d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016,4,16);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddPersonalActivity, person1, date);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddPersonalActivity, person2, date);

			var input = new AddPersonalActivityFormData
			{
				PersonIds = new[] { person1.Id.Value, person2.Id.Value },
				PersonalActivityId = Guid.NewGuid(),
				Date = date,
				StartTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndTime = new DateTime(2016, 4, 16, 17, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.AddPersonalActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotInvokeAddPersonalActivityCommandHandleWithoutPermission()
		{
			PermissionProvider.Enable();
			var person1 = PersonFactory.CreatePersonWithGuid("a","b");
			var person2 = PersonFactory.CreatePersonWithGuid("c","d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016,4,16);

			var input = new AddPersonalActivityFormData
			{
				PersonIds = new[] { person1.Id.Value, person2.Id.Value },
				PersonalActivityId = Guid.NewGuid(),
				Date = date,
				StartTime = new DateTime(2016, 4, 16, 8, 0, 0),
				EndTime = new DateTime(2016, 4, 16, 17, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.AddPersonalActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotRemoveActivityWithoutPermission()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a","b");
			PersonRepository.Has(person);

			var date = new DateOnly(2016,4,16);
			var input = new RemoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {new Guid(), new Guid()}
					},
					new PersonActivityItem
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
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);

			var date = new DateOnly(2016, 4, 16);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.RemoveActivity, person, date);

			var input = new RemoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {new Guid(), new Guid()}
					},
					new PersonActivityItem
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
		public void ShouldNotMoveActivityWhenNoMoveActivityPermission()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);

			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {new Guid()}
					}
				},
				Date = date,
				StartTime = new DateTime(2016, 4, 16, 10, 0,0)
			};
			ActivityCommandHandler.ResetCalledCount();
			var result = Target.MoveActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
			result.Count.Should().Be.EqualTo(1);
			result.First().Messages.Contains(Resources.NoPermissionMoveAgentActivity).Should().Be.True();
		}
		[Test]
		public void ShouldReturnWriteProtectedMsgWhenWriteProtected()
		{
			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.MoveActivity);
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			person.PersonWriteProtection.PersonWriteProtectedDate = date;

			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {new Guid()}
					}
				},
				Date = date,
				StartTime = new DateTime(2016, 4, 16, 10, 0,0)
			};
			ActivityCommandHandler.ResetCalledCount();
			var result = Target.MoveActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
			result.Count.Should().Be.EqualTo(1);
			result.First().Messages.Contains(Resources.WriteProtectSchedule).Should().Be.True();
		}
        
		[Test]
		public void ShouldInvokeMoveShiftLayerCommandWithPermission()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MoveActivity, person, date);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);
			
			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {personAss.ShiftLayers.First().Id.Value}
					}
				},
				Date = date,
				StartTime = new DateTime(2016, 4, 16, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			Target.MoveActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(1);
			((MoveShiftLayerCommand)(ActivityCommandHandler.CalledCommands.First())).NewStartTimeInUtc.Should().Be(input.StartTime);

		}
		[Test]
		public void ShouldCovertNewStartToUTC()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MoveActivity, person, date);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);
			
			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {personAss.ShiftLayers.First().Id.Value}
					}
				},
				Date = date,
				StartTime = new DateTime(2016, 4, 16, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			Target.MoveActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(1);
			((MoveShiftLayerCommand)(ActivityCommandHandler.CalledCommands.First())).NewStartTimeInUtc.Should().Be(TimeZoneHelper.ConvertToUtc(input.StartTime, TimeZoneInfo.Local));

		}
		[Test]
		public void ShouldInvokeCorrectCommandWithMultipleInputLayers()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MoveActivity, person, date);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			personAss.AddActivity(personAss.ShiftLayers.First().Payload, new DateTimePeriod(2016, 4, 16, 7, 2016, 4, 16, 10));
			personAss.AddActivity(personAss.ShiftLayers.First().Payload, new DateTimePeriod(2016, 4, 16, 9, 2016, 4, 16, 10));
			personAss.ShiftLayers.ForEach(x => x.WithId());
			PersonAssignmentRepo.Add(personAss);
			
			var input = new MoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {personAss.ShiftLayers.First().Id.Value, personAss.ShiftLayers.ToArray()[1].Id.Value, personAss.ShiftLayers.ToArray()[2].Id.Value}
					}
				},
				Date = date,
				StartTime = new DateTime(2016, 4, 16, 10, 0, 0)
			};
			ActivityCommandHandler.ResetCalledCount();
			Target.MoveActivity(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(3);
			
			((MoveShiftLayerCommand)(ActivityCommandHandler.CalledCommands.First())).NewStartTimeInUtc.Should().Be(TimeZoneHelper.ConvertToUtc(input.StartTime, TimeZoneInfo.Local).Add(TimeSpan.FromHours(1)));
			((MoveShiftLayerCommand)(ActivityCommandHandler.CalledCommands.ToArray()[1])).NewStartTimeInUtc.Should().Be(TimeZoneHelper.ConvertToUtc(input.StartTime, TimeZoneInfo.Local));
			((MoveShiftLayerCommand)(ActivityCommandHandler.CalledCommands.ToArray()[2])).NewStartTimeInUtc.Should().Be(TimeZoneHelper.ConvertToUtc(input.StartTime, TimeZoneInfo.Local).Add(TimeSpan.FromHours(2)));

		}

		[Test]
		public void ShouldReturnNoWriteProtectedAgentsIfHasModifyWriteProtectedSchedulePermission()
		{
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
			PermissionProvider.Enable();
			var agenta = PersonFactory.CreatePersonWithGuid("a","a");
			var agentb = PersonFactory.CreatePersonWithGuid("b","b");

			PersonRepository.Has(agenta);
			PersonRepository.Has(agentb);

			agenta.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016,1,1);
			agentb.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016,5,1);

			var result = Target.CheckWriteProtectedAgents(new DateOnly(2016,3,1),new[] { agenta.Id.Value,agentb.Id.Value });
			result.ToList().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotRemoveWriteProtectedActivity()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a","b");
			PersonRepository.Has(person);
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016,6,1);

			var date = new DateOnly(2016,4,16);
			var input = new RemoveActivityFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonActivities = new List<PersonActivityItem>
				{				
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> {new Guid()}
					}
				},
				Date = date
			};

			ActivityCommandHandler.ResetCalledCount();
			var results = Target.RemoveActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
			results.Count.Should().Be.EqualTo(1);
			results.First().Messages.Count.Should().Be.EqualTo(2);
			results.First().Messages[0].Should().Be.EqualTo(Resources.WriteProtectSchedule);
			results.First().Messages[1].Should().Be.EqualTo(Resources.NoPermissionRemoveAgentActivity);
		}

		[Test]
		public void ShouldNotAddActivityToWriteProtectedSchedule()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a","b");
			PersonRepository.Has(person);
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016,6,1);

			var date = new DateOnly(2016,4,16);

			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				Date = date,
				StartTime = new DateTime(2016,4,16,8,0,0),
				EndTime = new DateTime(2016,4,16,17,0,0),
				PersonIds = new[] { person.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();
			var results = Target.AddActivity(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);

			results.Count.Should().Be.EqualTo(1);
			results.First().Messages.Count.Should().Be.EqualTo(2);
			results.First().Messages[0].Should().Be.EqualTo(Resources.WriteProtectSchedule);
			results.First().Messages[1].Should().Be.EqualTo(Resources.NoPermissionAddAgentActivity);
		}
	}

	public class FakeActivityCommandHandler : IHandleCommand<AddActivityCommand>, IHandleCommand<AddPersonalActivityCommand>, IHandleCommand<RemoveActivityCommand>, IHandleCommand<MoveShiftLayerCommand>
	{
		private int calledCount;
		private IList<ITrackableCommand> commands = new List<ITrackableCommand>(); 
		public void Handle(AddActivityCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public int CalledCount
		{
			get { return calledCount; }
		}

		public IList<ITrackableCommand> CalledCommands
		{
			get { return commands; }
		} 
		public void ResetCalledCount()
		{
			calledCount = 0;
		}

		public void Handle(RemoveActivityCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(MoveShiftLayerCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(AddPersonalActivityCommand command)
		{
			calledCount++;
			commands.Add(command);
		}
	}
}
