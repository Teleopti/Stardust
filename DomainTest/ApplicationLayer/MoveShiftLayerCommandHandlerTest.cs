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
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[DomainTest]
	public class MoveShiftLayerCommandHandlerTest : ISetup
	{
		public MoveShiftLayerCommandHandler Target;
		public FakeScheduleStorage ScheduleStorage;
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleDifferenceSaver ScheduleDifferenceSaver;
		public FakeWriteSideRepository<IPerson> PersonRepository;
		public FakePersonSkillProvider PersonSkillProvider;
		public MutableNow Now;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeActivityRepository ActivityRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeWriteSideRepository<IPerson>>().For<IProxyForId<IPerson>>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
			system.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			system.UseTestDouble<MoveShiftLayerCommandHandler>().For<IHandleCommand<MoveShiftLayerCommand>>();
			system.UseTestDouble<FakePersonSkillProvider>().For<IPersonSkillProvider>();
		}

		[Test]
		public void ShouldReturnPersonAssignmentIsNotValidDotErrorIfPersonAssignmentNotExists()
		{
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

			var personAss = createPersonAssignmentWithOneLayer(activity, agent, CurrentScenario.Current(), new DateTime(2013, 11, 14, 8,0,0), new DateTime(2013, 11, 14, 16,0,0), TimeZoneInfo.Utc);
			ScheduleStorage.Add(personAss);
			
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
			var agent = new Person().WithId();
			PersonRepository.Add(agent);
			var activity = new Activity("act").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent,CurrentScenario.Current(), orgStart, orgEnd, TimeZoneInfo.Utc);

			ScheduleStorage.Add(personAss);
			
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
			var loadedPersonAss = ScheduleStorage.LoadAll().Single() as PersonAssignment;
			var modifiedLayer = loadedPersonAss.ShiftLayers.Single();
			modifiedLayer.Payload.Should().Be(activity);
			modifiedLayer.Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayer.Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
		}

		[Test]
		public void ShouldRaiseActivityMovedEvent()
		{
			var agent = new Person().WithId();
			PersonRepository.Add(agent);
			var activity = new Activity("act").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent,CurrentScenario.Current(), orgStart, orgEnd, TimeZoneInfo.Utc);
			ScheduleStorage.Add(personAss);
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
			var loadedPersonAss = ScheduleStorage.LoadAll().Single() as PersonAssignment;
			var @event = loadedPersonAss.PopAllEvents().OfType<ActivityMovedEvent>().Single();
			@event.PersonId.Should().Be(PersonRepository.Single().Id.Value);
			@event.StartDateTime.Should().Be(expectedStartInUtc);
			@event.ScenarioId.Should().Be(loadedPersonAss.Scenario.Id.Value);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(CurrentScenario.Current().BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldPersistDeltas()
		{
			var agent = new Person().WithId();
			PersonRepository.Add(agent);

			IntervalLengthFetcher.Has(15);
			Now.Is(new DateTime(2013, 11, 14, 0, 0, 0, DateTimeKind.Utc));
			var skill = SkillFactory.CreateSkillWithId("skill", 15);
			PersonSkillProvider.SkillCombination = new SkillCombination(new[] { skill }, new DateOnlyPeriod(), null, new[] { skill });

			var activity = new Activity("act").WithId();
			activity.RequiresSkill = true;
			skill.Activity = activity;

			ActivityRepository.Add(activity);

			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, CurrentScenario.Current(), orgStart, orgEnd, TimeZoneInfo.Utc);

			ScheduleStorage.Add(personAss);

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
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository();
			var scenario = new ThisCurrentScenario(new Scenario("scenario"));
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var target = new MoveShiftLayerCommandHandlerNoDeltas( personAssignmentRepository, personRepository, scenario);

			var cmd = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = new DateOnly(DateTime.UtcNow),
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
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
                PersonAssignmentFactory.CreateAssignmentWithMainShift(agent,
                    activity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16))
            };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
            
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
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

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				personAss
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
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

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				personAss
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
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