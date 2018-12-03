using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
	[TestFixture]
	public class SdkProjectionServiceFactoryTest
	{
		private ISdkProjectionServiceFactory target;
		private IScheduleDay scheduleDay;

		[SetUp]
		public void Setup()
		{
			target = new SdkProjectionServiceFactory();
			var person = new Person();
			var scenario = new Scenario("d");
			var dic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(1900, 1, 1, 2200, 1, 1)), new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), CurrentAuthorization.Make());
			scheduleDay = ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2000, 1, 1), CurrentAuthorization.Make());
			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			var abs = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			scheduleDay.Add(ass);
			scheduleDay.Add(abs);
		}

		[Test]
		public void ShouldReturnCorrectMergerUsingCorrectScheduleDayWhenMidnightSplitIsUsed()
		{
			const string specialProjection = "midnightSplit";
			var timeZoneInfo = TimeZoneInfo.Local;
			var projService = (ScheduleProjectionService)target.CreateProjectionService(scheduleDay, specialProjection, timeZoneInfo);

			projService.ProjectionMerger.Should().Be.InstanceOf<ProjectionMidnightSplitterMerger>();
			projService.ScheduleDay.Should().Be.SameInstanceAs(scheduleDay);
		}

		[Test]
		public void ShouldReturnCorrectMergerUsingCorrectScheduleDayWhenExcludeAbsencesMidnightSplitIsUsed()
		{
			const string specialProjection = "excludeAbsencesMidnightSplit";
			var timeZoneInfo = TimeZoneInfo.Local;
			var projService = (ScheduleProjectionService)target.CreateProjectionService(scheduleDay, specialProjection, timeZoneInfo);

			projService.ProjectionMerger.Should().Be.InstanceOf<ProjectionMidnightSplitterMerger>();
			projService.ScheduleDay.Should().Not.Be.SameInstanceAs(scheduleDay);
			projService.ScheduleDay.PersonAbsenceCollection().Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnCorrectMergerUsingCorrectScheduleDayWhenExcludeAbsenceIsUsed()
		{
			const string specialProjection = "excludeAbsences";
			var timeZoneInfo = TimeZoneInfo.Local;
			var projService = (ScheduleProjectionService)target.CreateProjectionService(scheduleDay, specialProjection, timeZoneInfo);

			projService.ProjectionMerger.Should().Be.InstanceOf<ProjectionPayloadMerger>();
			projService.ScheduleDay.Should().Not.Be.SameInstanceAs(scheduleDay);
			projService.ScheduleDay.PersonAbsenceCollection().Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnCorrectMergerUsingCorrectScheduleDayWhenBlankIsUsed()
		{
			var specialProjection = string.Empty;
			var timeZoneInfo = TimeZoneInfo.Local;
			var projService = (ScheduleProjectionService)target.CreateProjectionService(scheduleDay, specialProjection, timeZoneInfo);

			projService.ProjectionMerger.Should().Be.InstanceOf<ProjectionPayloadMerger>();
			projService.ScheduleDay.Should().Be.SameInstanceAs(scheduleDay);
		}

		[Test]
		public void ShouldReturnSameDataAsStringEmptyIfStrangeValueIsUsed()
		{
			//throw or default? Choose default here....
			const string specialProjection = "arne anka";
			var timeZoneInfo = TimeZoneInfo.Local;
			var projService = (ScheduleProjectionService)target.CreateProjectionService(scheduleDay, specialProjection, timeZoneInfo);

			projService.ProjectionMerger.Should().Be.InstanceOf<ProjectionPayloadMerger>();
			projService.ScheduleDay.Should().Be.SameInstanceAs(scheduleDay);
		}

		[Test]
		public void ShouldReturnSameDataAsStringEmptyIfNullValueIsUsed()
		{
			//throw or default? Choose default here....
			const string specialProjection = null;
			var timeZoneInfo = TimeZoneInfo.Local;
			var projService = (ScheduleProjectionService)target.CreateProjectionService(scheduleDay, specialProjection, timeZoneInfo);

			projService.ProjectionMerger.Should().Be.InstanceOf<ProjectionPayloadMerger>();
			projService.ScheduleDay.Should().Be.SameInstanceAs(scheduleDay);
		}
	}
}