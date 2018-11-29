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
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[DomainTest]
	public class MoveShiftCommandHandlerTest : IIsolateSystem
	{
		public MoveShiftCommandHandler Target;
		public IScheduleStorage ScheduleStorage;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public MutableNow Now;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			isolate.UseTestDouble<MoveShiftCommandHandler>().For<IHandleCommand<MoveShiftCommand>>();
		}

		[Test]
		public void ShouldReturnPersonAssignmentIsNotValidDotErrorIfPersonAssignmentNotExists()
		{
			ScenarioRepository.Has("Default");
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
			
			var cmd = new MoveShiftCommand
			{
				PersonId = agent.Id.GetValueOrDefault(),
				ScheduleDate = new DateOnly(2013, 11, 14),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;
			var personAssignment = PersonAssignmentRepository.LoadAll().Single();
			var modifiedLayer = personAssignment.ShiftLayers.Single();
			modifiedLayer.Payload.Should().Be(activity);
			modifiedLayer.Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayer.Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
		}


		[Test]
		public void ShouldMoveShiftWithOvertimeActivity()
		{
			var scenario = ScenarioRepository.Has("Default");
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var overtimeAct = new Activity("overtime").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			PersonRepository.Add(agent);

			var personAss = createPersonAssignmentWithOneLayer(activity, agent, scenario, orgStart, orgEnd, TimeZoneInfo.Utc);

			personAss.AddOvertimeActivity(overtimeAct, new DateTimePeriod(createDateTimeUtc(11), createDateTimeUtc(12)), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));

			PersonAssignmentRepository.Add(personAss);
			
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
			var personAssignment = PersonAssignmentRepository.LoadAll().Single();
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
			var scenario = ScenarioRepository.Has("Default");
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personalActivity = new Activity("personal").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			PersonRepository.Add(agent);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, scenario, orgStart, orgEnd, TimeZoneInfo.Utc);
			personAss.AddPersonalActivity(personalActivity, new DateTimePeriod(createDateTimeUtc(11), createDateTimeUtc(12)));

			PersonAssignmentRepository.Add(personAss);
			
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
			var personAssignment = PersonAssignmentRepository.LoadAll().Single();
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
			var scenario = ScenarioRepository.Has("Default");
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personalActivity = new Activity("personal").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			PersonRepository.Add(agent);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, scenario, orgStart, orgEnd, TimeZoneInfo.Utc);
			personAss.AddPersonalActivity(personalActivity, new DateTimePeriod(createDateTimeUtc(11), createDateTimeUtc(12)));

			PersonAssignmentRepository.Add(personAss);
			
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

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();
			var events = personAssignment.PopAllEvents();
			events.Single().Should().Be.OfType<ActivityMovedEvent>();
		}

		[Test]
		public void ShouldNotCountPersonalActivityWhenCalculatingNewShiftStartTime()
		{
			var scenario = ScenarioRepository.Has("Default");
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personalActivity = new Activity("personal").WithId();
			var orgStart = createDateTimeUtc(8);
			var orgEnd = createDateTimeUtc(17);
			var userTimeZone = new UtcTimeZone();
			agent.PermissionInformation.SetDefaultTimeZone(userTimeZone.TimeZone());
			PersonRepository.Add(agent);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, scenario, orgStart, orgEnd, TimeZoneInfo.Utc);
			personAss.AddPersonalActivity(personalActivity, new DateTimePeriod(createDateTimeUtc(7), createDateTimeUtc(8)));

			PersonAssignmentRepository.Add(personAss);
			
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

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();
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
			var scenario = ScenarioRepository.Has("Default");
			IntervalLengthFetcher.Has(15);
			Now.Is(new DateTime(2013, 11, 14,0,0,0,DateTimeKind.Utc));
			var skill = SkillFactory.CreateSkillWithId("skill", 15);
			SkillRepository.Add(skill);
			var agent = PersonRepository.Has(skill);
			var activity = new Activity("act").WithId();
			skill.Activity = activity;
			activity.RequiresSkill = true;
			ActivityRepository.Add(activity);

			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, scenario, orgStart, orgEnd, TimeZoneInfo.Utc);
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
}
