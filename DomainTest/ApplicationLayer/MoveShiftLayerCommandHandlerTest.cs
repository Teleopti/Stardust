using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[DomainTest]
	public class MoveShiftLayerCommandHandlerTest : ISetup
	{
		public MoveShiftLayerCommandHandler Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScheduleDifferenceSaver_DoNotUse ScheduleDifferenceSaver;
		public FakePersonRepository PersonRepository;
		public MutableNow Now;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeScheduleDifferenceSaver_DoNotUse>().For<IScheduleDifferenceSaver>();
			system.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			system.UseTestDouble<MoveShiftLayerCommandHandler>().For<IHandleCommand<MoveShiftLayerCommand>>();
		}

		[Test]
		public void ShouldReturnPersonAssignmentIsNotValidDotErrorIfPersonAssignmentNotExists()
		{
			ScenarioRepository.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			var cmd = new MoveShiftLayerCommand
			{
				AgentId = person.Id.GetValueOrDefault(),
				ScheduleDate = DateOnly.Today,
				NewStartTimeInUtc = new DateTime(2000, 1, 1, 4, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(cmd);
			cmd.ErrorMessages.Should().Contain(Resources.PersonAssignmentIsNotValid);
		}

		[Test]
		public void ShouldReturnShiftLayerNotExistErrorIfShiftLayerNotExists()
		{
			var agent = new Person().WithId();
			PersonRepository.Add(agent);
			var activity = new Activity("act").WithId();
			var scenario = ScenarioRepository.Has("Default");

			var personAss = createPersonAssignmentWithOneLayer(activity, agent, scenario, new DateTime(2013, 11, 14, 8,0,0), new DateTime(2013, 11, 14, 16,0,0), TimeZoneInfo.Utc);
			PersonAssignmentRepository.Add(personAss);
			
			var cmd = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.GetValueOrDefault(),
				ScheduleDate = new DateOnly(2013, 11, 14),
				ShiftLayerId = Guid.NewGuid(),
				NewStartTimeInUtc = new DateTime(2013, 11, 14, 9, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(cmd);
			cmd.ErrorMessages.Should().Contain(Resources.NoShiftsFound);
		}

		[Test]
		public void ShouldMoveShiftLayer()
		{
			var scenario = ScenarioRepository.Has("Default");
			var agent = new Person().WithId();
			PersonRepository.Add(agent);
			var activity = new Activity("act").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent,scenario, orgStart, orgEnd, TimeZoneInfo.Utc);

			PersonAssignmentRepository.Add(personAss);
			
			var shiftLayer = personAss.ShiftLayers.Single();
			shiftLayer.WithId();

			var cmd = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.GetValueOrDefault(),
				ScheduleDate = new DateOnly(2013, 11, 14),
				ShiftLayerId = shiftLayer.Id.GetValueOrDefault(),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;
			var loadedPersonAss = PersonAssignmentRepository.LoadAll().Single();
			var modifiedLayer = loadedPersonAss.ShiftLayers.Single();
			modifiedLayer.Payload.Should().Be(activity);
			modifiedLayer.Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayer.Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
		}

		[Test]
		public void ShouldRaiseActivityMovedEvent()
		{
			var scenario = ScenarioRepository.Has("Default");
			var agent = new Person().WithId();
			PersonRepository.Add(agent);
			var activity = new Activity("act").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, scenario, orgStart, orgEnd, TimeZoneInfo.Utc);
			PersonAssignmentRepository.Add(personAss);
			var shiftLayer = personAss.ShiftLayers.Single();
			shiftLayer.WithId();
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.GetValueOrDefault(),
				ScheduleDate = new DateOnly(2013, 11, 14),
				ShiftLayerId = shiftLayer.Id.GetValueOrDefault(),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			Target.Handle(command);
			var expectedStartInUtc = command.NewStartTimeInUtc;
			var loadedPersonAss = PersonAssignmentRepository.LoadAll().Single();
			var @event = loadedPersonAss.PopAllEvents().OfType<ActivityMovedEvent>().Single();
			@event.PersonId.Should().Be(agent.Id.Value);
			@event.StartDateTime.Should().Be(expectedStartInUtc);
			@event.ScenarioId.Should().Be(loadedPersonAss.Scenario.Id.Value);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldPersistDeltas()
		{
			var scenario = ScenarioRepository.Has("Default");
			IntervalLengthFetcher.Has(15);
			Now.Is(new DateTime(2013, 11, 14, 0, 0, 0, DateTimeKind.Utc));
			var skill = SkillFactory.CreateSkillWithId("skill", 15);
			SkillRepository.Add(skill);
			var activity = new Activity("act").WithId();
			activity.RequiresSkill = true;
			skill.Activity = activity;
			var agent = PersonRepository.Has(skill);

			ActivityRepository.Add(activity);

			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, scenario, orgStart, orgEnd, TimeZoneInfo.Utc);

			PersonAssignmentRepository.Add(personAss);

			var shiftLayer = personAss.ShiftLayers.Single();
			shiftLayer.WithId();

			var cmd = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.GetValueOrDefault(),
				ScheduleDate = new DateOnly(2013, 11, 14),
				ShiftLayerId = shiftLayer.Id.GetValueOrDefault(),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;

			var combs = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2013, 11, 14, 2013, 11, 15), false).ToList();
			combs.Count.Should().Be.EqualTo(32);
			combs.Count(x => x.StartDateTime < expectedStart).Should().Be.EqualTo(0);
			combs.Count(x => x.EndDateTime > orgEnd).Should().Be.EqualTo(0);
		}

		private static DateTime createDateTimeUtc(int hourOnDay)
		{
			return new DateTime(2013, 11, 14, hourOnDay, 0, 0, 0, DateTimeKind.Utc);
		}

		private static IPersonAssignment createPersonAssignmentWithOneLayer(IActivity activity, IPerson agent, IScenario current, DateTime orgStart, DateTime orgEnd, TimeZoneInfo timeZone)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, current,
				activity, new DateTimePeriod(TimeZoneHelper.ConvertToUtc(orgStart, timeZone),
					TimeZoneHelper.ConvertToUtc(orgEnd, timeZone)), new ShiftCategory("katt"));
			return assignment;
		}
	}

	[TestWithStaticDependenciesAvoidUse]
	public class MoveShiftLayerCommandHandlerNoDeltasTest
	{
		[Test]
		public void ShouldReturnPersonAssignmentIsNotValidDotErrorIfPersonAssignmentNotExists()
		{
			var agent = new Person().WithId();
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository(null);
			var scenario = new ThisCurrentScenario(new Scenario("scenario"));
			var personRepository = new FakeWriteSideRepository<IPerson>(null) { agent };
			var target = new MoveShiftLayerCommandHandlerNoDeltas( personAssignmentRepository, personRepository, scenario);

			var cmd = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = DateOnly.Today,
				ShiftLayerId = Guid.NewGuid(),
				NewStartTimeInUtc = new DateTime(2000, 1, 1, 4, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			target.Handle(cmd);
			cmd.ErrorMessages.Should().Contain(Resources.PersonAssignmentIsNotValid);
		}
		[Test]
		public void ShouldReturnShiftLayerNotExistErrorIfShiftLayerNotExists()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository(null)
			{
                PersonAssignmentFactory.CreateAssignmentWithMainShift(agent,
                    activity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16))
            };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
            
			var personRepository = new FakeWriteSideRepository<IPerson>(null) { agent };
			var target = new MoveShiftLayerCommandHandlerNoDeltas( personAssignmentRepository, personRepository, scenario);

			var cmd = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
				ShiftLayerId = Guid.NewGuid(),
				NewStartTimeInUtc = new DateTime(2013, 11, 14, 9, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			target.Handle(cmd);
			cmd.ErrorMessages.Should().Contain(Resources.NoShiftsFound);
		}

		[Test]
		public void ShouldMoveShiftLayer()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = UserTimeZone.Make();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, orgStart, orgEnd, userTimeZone);

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository(null)
			{
				personAss
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson>(null) { agent };
			var personAssignment = personAssignmentRepository.Single();
			var shiftLayer = personAssignment.ShiftLayers.Single();
			shiftLayer.WithId();
			var target = new MoveShiftLayerCommandHandlerNoDeltas(personAssignmentRepository, personRepository, scenario);

			var cmd = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
				ShiftLayerId = shiftLayer.Id.GetValueOrDefault(),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;
			var modifiedLayer = personAssignmentRepository.Single().ShiftLayers.Single();
			modifiedLayer.Payload.Should().Be(activity);
			modifiedLayer.Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayer.Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
		}

		[Test]
		public void ShouldRaiseActivityMovedEvent()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = UserTimeZone.Make();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, orgStart, orgEnd, userTimeZone);

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository(null)
			{
				personAss
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson>(null) { agent };
			var personAssignment = personAssignmentRepository.Single();
			var shiftLayer = personAssignment.ShiftLayers.Single();
			shiftLayer.WithId();
			var target = new MoveShiftLayerCommandHandlerNoDeltas(personAssignmentRepository, personRepository, scenario);
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
				ShiftLayerId = shiftLayer.Id.GetValueOrDefault(),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			target.Handle(command);
			var expectedStartInUtc = command.NewStartTimeInUtc;
			var @event = personAssignmentRepository.Single().PopAllEvents().OfType<ActivityMovedEvent>().Single();
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.StartDateTime.Should().Be(expectedStartInUtc);
			@event.ScenarioId.Should().Be(personAssignmentRepository.Single().Scenario.Id.Value);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.Current().BusinessUnit.Id.GetValueOrDefault());
		}

		private static DateTime createDateTimeUtc(int hourOnDay)
		{
			return new DateTime(2013, 11, 14, hourOnDay, 0, 0, 0, DateTimeKind.Utc);
		}

		private static IPersonAssignment createPersonAssignmentWithOneLayer(IActivity activity, IPerson agent, DateTime orgStart, DateTime orgEnd, IUserTimeZone userTimeZone)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent,
				activity, new DateTimePeriod(TimeZoneHelper.ConvertToUtc(orgStart, userTimeZone.TimeZone()),
					TimeZoneHelper.ConvertToUtc(orgEnd, userTimeZone.TimeZone())));
			return assignment;
		}
	}
}