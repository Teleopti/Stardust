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
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
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
	public class MoveShiftCommandHandlerTest : ISetup
	{
		public MoveShiftCommandHandler Target;
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
			system.UseTestDouble<MoveShiftCommandHandler>().For<IHandleCommand<MoveShiftCommand>>();
			system.UseTestDouble<FakePersonSkillProvider>().For<IPersonSkillProvider>();
		}

		[Test]
		public void ShouldReturnPersonAssignmentIsNotValidDotErrorIfPersonAssignmentNotExists()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			
			var cmd = new MoveShiftCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScheduleDate = DateOnly.Today,
				NewStartTimeInUtc = new DateTime(2000, 1, 1, 4, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(cmd);
			cmd.ErrorMessages.Should().Contain(Resources.PersonAssignmentIsNotValid);
		}

		[Test]
		public void ShouldMoveShift()
		{
			var agent = new Person().WithId();
			PersonRepository.Add(agent);
			var activity = new Activity("act").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, CurrentScenario.Current(),  orgStart, orgEnd, TimeZoneInfo.Utc);
			ScheduleStorage.Add(personAss);

			var shiftLayer = personAss.ShiftLayers.Single();
			shiftLayer.WithId();
			
			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.GetValueOrDefault(),
				ScheduleDate = new DateOnly(2013, 11, 14),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;
			var personAssignment = ScheduleStorage.LoadAll().Single() as PersonAssignment;
			var modifiedLayer = personAssignment.ShiftLayers.Single();
			modifiedLayer.Payload.Should().Be(activity);
			modifiedLayer.Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayer.Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
		}


		[Test]
		public void ShouldMoveShiftWithOvertimeActivity()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var overtimeAct = new Activity("overtime").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			PersonRepository.Add(agent);

			var personAss = createPersonAssignmentWithOneLayer(activity, agent, CurrentScenario.Current(), orgStart, orgEnd, TimeZoneInfo.Utc);

			personAss.AddOvertimeActivity(overtimeAct, new DateTimePeriod(createDateTimeUtc(11), createDateTimeUtc(12)), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));

			ScheduleStorage.Add(personAss);
			
			var shiftLayers = personAss.ShiftLayers.ToList();
			shiftLayers.ForEach(l => l.WithId());

			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.GetValueOrDefault(),
				ScheduleDate = new DateOnly(2013, 11, 14),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;
			var personAssignment = ScheduleStorage.LoadAll().Single() as PersonAssignment;
			var modifiedLayers = personAssignment.ShiftLayers.ToList();

			modifiedLayers[0].Payload.Should().Be(activity);
			modifiedLayers[0].Should().Be.OfType<MainShiftLayer>();
			modifiedLayers[0].Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayers[0].Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
			modifiedLayers[1].Payload.Should().Be(overtimeAct);
			modifiedLayers[1].Period.StartDateTime.Should().Be(createDateTimeUtc(7));
			modifiedLayers[1].Period.EndDateTime.Should().Be(createDateTimeUtc(8));
			modifiedLayers[1].Should().Be.OfType<OvertimeShiftLayer>();
		}

		[Test]
		public void ShouldNotMoveShiftWithPersonalActivity()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personalActivity = new Activity("personal").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			PersonRepository.Add(agent);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, CurrentScenario.Current(), orgStart, orgEnd, TimeZoneInfo.Utc);
			personAss.AddPersonalActivity(personalActivity, new DateTimePeriod(createDateTimeUtc(11), createDateTimeUtc(12)));

			ScheduleStorage.Add(personAss);
			
			var shiftLayers = personAss.ShiftLayers.ToList();
			shiftLayers.ForEach(l => l.WithId());
			
			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.GetValueOrDefault(),
				ScheduleDate = new DateOnly(2013, 11, 14),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;
			var personAssignment = ScheduleStorage.LoadAll().Single() as PersonAssignment;
			var modifiedLayers = personAssignment.ShiftLayers.ToList();

			modifiedLayers[0].Payload.Should().Be(activity);
			modifiedLayers[0].Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayers[0].Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
			modifiedLayers[1].Payload.Should().Be(personalActivity);
			modifiedLayers[1].Period.StartDateTime.Should().Be(createDateTimeUtc(11));
			modifiedLayers[1].Period.EndDateTime.Should().Be(createDateTimeUtc(12));
		}

		[Test]
		public void ShouldTriggerOneEvent()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personalActivity = new Activity("personal").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			PersonRepository.Add(agent);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, CurrentScenario.Current(), orgStart, orgEnd, TimeZoneInfo.Utc);
			personAss.AddPersonalActivity(personalActivity, new DateTimePeriod(createDateTimeUtc(11), createDateTimeUtc(12)));

			ScheduleStorage.Add(personAss);
			
			var shiftLayers = personAss.ShiftLayers.ToList();
			shiftLayers.ForEach(l => l.WithId());
			
			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.GetValueOrDefault(),
				ScheduleDate = new DateOnly(2013, 11, 14),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			personAss.PopAllEvents();
			Target.Handle(cmd);

			var personAssignment = ScheduleStorage.LoadAll().Single() as PersonAssignment;
			var events = personAssignment.PopAllEvents();
			events.Single().Should().Be.OfType<ActivityMovedEvent>();
		}

		[Test]
		public void ShouldNotCountPersonalActivityWhenCalculatingNewShiftStartTime()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personalActivity = new Activity("personal").WithId();
			var orgStart = createDateTimeUtc(8);
			var orgEnd = createDateTimeUtc(17);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			PersonRepository.Add(agent);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, CurrentScenario.Current(), orgStart, orgEnd, TimeZoneInfo.Utc);
			personAss.AddPersonalActivity(personalActivity, new DateTimePeriod(createDateTimeUtc(7), createDateTimeUtc(8)));

			ScheduleStorage.Add(personAss);
			

			var shiftLayers = personAss.ShiftLayers.ToList();
			shiftLayers.ForEach(l => l.WithId());
			
			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.GetValueOrDefault(),
				ScheduleDate = new DateOnly(2013, 11, 14),
				NewStartTimeInUtc = createDateTimeUtc(10),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;

			var personAssignment = ScheduleStorage.LoadAll().Single() as PersonAssignment;
			var modifiedLayers = personAssignment.ShiftLayers.ToList();
			modifiedLayers[0].Payload.Should().Be(activity);
			modifiedLayers[0].Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayers[0].Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
			modifiedLayers[1].Payload.Should().Be(personalActivity);
			modifiedLayers[1].Period.StartDateTime.Should().Be(createDateTimeUtc(7));
			modifiedLayers[1].Period.EndDateTime.Should().Be(createDateTimeUtc(8));
		}

		[Test]
		public void ShouldSaveDeltas()
		{
			IntervalLengthFetcher.Has(15);
			Now.Is(new DateTime(2013, 11, 14,0,0,0,DateTimeKind.Utc));
			var skill = SkillFactory.CreateSkillWithId("skill", 15);
			PersonSkillProvider.SkillCombination = new SkillCombination(new[] { skill}, new DateOnlyPeriod(), null, new[] { skill });

			var agent = new Person().WithId();
			PersonRepository.Add(agent);
			var activity = new Activity("act").WithId();
			skill.Activity = activity;
			activity.RequiresSkill = true;
			ActivityRepository.Add(activity);

			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, CurrentScenario.Current(), orgStart, orgEnd, TimeZoneInfo.Utc);
			ScheduleStorage.Add(personAss);

			var shiftLayer = personAss.ShiftLayers.Single();
			shiftLayer.WithId();

			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
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
	public class MoveShiftCommandHandlerNoDeltasTest
	{
		[Test]
		public void ShouldReturnPersonAssignmentIsNotValidDotErrorIfPersonAssignmentNotExists()
		{
			var agent = new Person().WithId();
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository();
			var scenario = new ThisCurrentScenario(new Scenario("scenario"));
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var target = new MoveShiftCommandHandlerNoDeltas(personRepository, personAssignmentRepository, scenario);

			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.Value,
				ScheduleDate = DateOnly.Today,
				NewStartTimeInUtc = new DateTime(2000, 1, 1, 4, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			target.Handle(cmd);
			cmd.ErrorMessages.Should().Contain(Resources.PersonAssignmentIsNotValid);
		}

		[Test]
		public void ShouldChangeState()
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
			var target = new MoveShiftCommandHandlerNoDeltas(personRepository, personAssignmentRepository, scenario);

			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
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
		public void ShouldChangeStateWithOvertimeActivity()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var overtimeAct = new Activity("overtime").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, orgStart, orgEnd, userTimeZone);
			personAss.AddOvertimeActivity(overtimeAct, new DateTimePeriod(createDateTimeUtc(11), createDateTimeUtc(12)), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				personAss
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var personAssignment = personAssignmentRepository.Single();
			var shiftLayers = personAssignment.ShiftLayers.ToList();
			shiftLayers.ForEach(l => l.WithId());
			var target = new MoveShiftCommandHandlerNoDeltas(personRepository, personAssignmentRepository, scenario);

			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;
			var modifiedLayers = personAssignmentRepository.Single().ShiftLayers.ToList();

			modifiedLayers[0].Payload.Should().Be(activity);
			modifiedLayers[0].Should().Be.OfType<MainShiftLayer>();
			modifiedLayers[0].Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayers[0].Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
			modifiedLayers[1].Payload.Should().Be(overtimeAct);
			modifiedLayers[1].Period.StartDateTime.Should().Be(createDateTimeUtc(7));
			modifiedLayers[1].Period.EndDateTime.Should().Be(createDateTimeUtc(8));
			modifiedLayers[1].Should().Be.OfType<OvertimeShiftLayer>();
		}

		[Test]
		public void ShouldNotChangeStateWithPersonalActivity()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personalActivity = new Activity("personal").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, orgStart, orgEnd, userTimeZone);
			personAss.AddPersonalActivity(personalActivity, new DateTimePeriod(createDateTimeUtc(11), createDateTimeUtc(12)));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				personAss
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var personAssignment = personAssignmentRepository.Single();
			var shiftLayers = personAssignment.ShiftLayers.ToList();
			shiftLayers.ForEach(l => l.WithId());
			var target = new MoveShiftCommandHandlerNoDeltas(personRepository, personAssignmentRepository, scenario);

			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;
			var modifiedLayers = personAssignmentRepository.Single().ShiftLayers.ToList();
			modifiedLayers[0].Payload.Should().Be(activity);
			modifiedLayers[0].Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayers[0].Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
			modifiedLayers[1].Payload.Should().Be(personalActivity);
			modifiedLayers[1].Period.StartDateTime.Should().Be(createDateTimeUtc(11));
			modifiedLayers[1].Period.EndDateTime.Should().Be(createDateTimeUtc(12));
		}

		[Test]
		public void ShouldTriggerOneEvent()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personalActivity = new Activity("personal").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, orgStart, orgEnd, userTimeZone);
			personAss.AddPersonalActivity(personalActivity, new DateTimePeriod(createDateTimeUtc(11), createDateTimeUtc(12)));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				personAss
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var personAssignment = personAssignmentRepository.Single();
			var shiftLayers = personAssignment.ShiftLayers.ToList();
			shiftLayers.ForEach(l => l.WithId());
			var target = new MoveShiftCommandHandlerNoDeltas(personRepository, personAssignmentRepository, scenario);

			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			personAssignment.PopAllEvents();
			target.Handle(cmd);

			var events = personAssignmentRepository.Single().PopAllEvents();
			events.Single().Should().Be.OfType<ActivityMovedEvent>();
		}

		[Test]
		public void ShouldNotCountPersonalActivityWhenCalculatingNewShiftStartTime()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personalActivity = new Activity("personal").WithId();
			var orgStart = createDateTimeUtc(8);
			var orgEnd = createDateTimeUtc(17);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, orgStart, orgEnd, userTimeZone);
			personAss.AddPersonalActivity(personalActivity, new DateTimePeriod(createDateTimeUtc(7), createDateTimeUtc(8)));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				personAss
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var personAssignment = personAssignmentRepository.Single();
			var shiftLayers = personAssignment.ShiftLayers.ToList();
			shiftLayers.ForEach(l => l.WithId());
			var target = new MoveShiftCommandHandlerNoDeltas(personRepository, personAssignmentRepository, scenario);

			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
				NewStartTimeInUtc = createDateTimeUtc(10),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;

			var modifiedLayers = personAssignmentRepository.Single().ShiftLayers.ToList();
			modifiedLayers[0].Payload.Should().Be(activity);
			modifiedLayers[0].Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayers[0].Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
			modifiedLayers[1].Payload.Should().Be(personalActivity);
			modifiedLayers[1].Period.StartDateTime.Should().Be(createDateTimeUtc(7));
			modifiedLayers[1].Period.EndDateTime.Should().Be(createDateTimeUtc(8));
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
