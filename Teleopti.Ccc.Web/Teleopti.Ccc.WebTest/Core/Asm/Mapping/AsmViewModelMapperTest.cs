using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Core.Asm.Mapping
{
	[TestFixture]
	[MyTimeWebTest]
	public class AsmViewModelMapperTest : IIsolateSystem
	{
		public IScheduleStorage ScheduleStorage;
		public IAsmViewModelMapper Target;
		public FakeLoggedOnUser LoggedOnUser;
		public ICurrentScenario CurrentScenario;
		public FakeUserTimeZone UserTimeZone;
		public MutableNow Now;

		private static readonly CultureInfo defaultCulture = CultureInfo.GetCultureInfo("sv-SE");

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario { DefaultScenario = true })).For<IScenarioRepository>();
		}

		[Test]
		public void ShouldReturnALayerForEachlayerInProjection()
		{
			setNow();

			var person = setLoggedOnUser();

			var yesterDayPeriod = new DateTimePeriod(2018, 3, 22, 8, 2018, 3, 22, 17);
			var activity1 = new Activity("1").WithId();
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(), activity1, yesterDayPeriod, new ShiftCategory()));

			var todayDayPeriod = new DateTimePeriod(2018, 3, 23, 8, 2018, 3, 23, 17);
			var activity2 = new Activity("2").WithId();
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(), activity2, todayDayPeriod, new ShiftCategory()));

			var tomorrowDayPeriod = new DateTimePeriod(2018, 3, 24, 8, 2018, 3, 24, 17);
			var activity3 = new Activity("3").WithId();
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(), activity3, tomorrowDayPeriod, new ShiftCategory()));

			var datePeriod = new DateOnlyPeriod(2018, 3, 22, 2018, 3, 24);
			var scheduleDays = getScheduleDays(datePeriod);

			var result = Target.Map(new DateTime(2018, 3, 22), scheduleDays, 0).Layers.ToList();

			result.Count.Should().Be.EqualTo(3);
			result.Any(l => l.Payload.Equals("1")).Should().Be.True();
			result.Any(l => l.Payload.Equals("2")).Should().Be.True();
			result.Any(l => l.Payload.Equals("3")).Should().Be.True();
		}

		[Test]
		public void ShouldSetStart()
		{
			setNow();

			var person = setLoggedOnUser();

			var yesterDayPeriod = new DateTimePeriod(2018, 3, 22, 8, 2018, 3, 22, 17);
			var activity1 = new Activity("1").WithId();
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(), activity1, yesterDayPeriod, new ShiftCategory()));

			var datePeriod = new DateOnlyPeriod(2018, 3, 22, 2018, 3, 24);
			var scheduleDays = getScheduleDays(datePeriod);

			var result = Target.Map(new DateTime(2018, 3, 22), scheduleDays, 0);

			result.Layers.First().StartMinutesSinceAsmZero.Should().Be.EqualTo(540);
		}

		[Test]
		public void ShouldSetEnd()
		{
			setNow();

			var person = setLoggedOnUser();

			var yesterDayPeriod = new DateTimePeriod(2018, 3, 22, 8, 2018, 3, 22, 17);
			var activity1 = new Activity("1").WithId();
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(), activity1, yesterDayPeriod, new ShiftCategory()));

			var datePeriod = new DateOnlyPeriod(2018, 3, 22, 2018, 3, 24);
			var scheduleDays = getScheduleDays(datePeriod);

			var result = Target.Map(new DateTime(2018, 3, 22), scheduleDays, 0);

			result.Layers.First().LengthInMinutes.Should().Be.EqualTo(540);
		}

		[Test]
		public void ShouldSetLayerColor()
		{
			setNow();

			var color = Color.BlanchedAlmond;
			var person = setLoggedOnUser();

			var yesterDayPeriod = new DateTimePeriod(2018, 3, 22, 8, 2018, 3, 22, 17);
			var activity1 = new Activity("1").WithId();
			activity1.DisplayColor = color;
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(),
				activity1, yesterDayPeriod, new ShiftCategory()));

			var datePeriod = new DateOnlyPeriod(2018, 3, 22, 2018, 3, 24);
			var scheduleDays = getScheduleDays(datePeriod);

			var result = Target.Map(new DateTime(2018, 3, 22), scheduleDays, 0);

			result.Layers.First().Color.Should().Be.EqualTo(ColorTranslator.ToHtml(color));
		}

		[Test]
		public void ShouldSetStartTimeText()
		{
			setNow();

			var person = setLoggedOnUser();

			var yesterDayPeriod = new DateTimePeriod(2018, 3, 22, 8, 2018, 3, 22, 17).ChangeStartTime(TimeSpan.FromMinutes(15));
			var activity1 = new Activity("1").WithId();
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(), activity1, yesterDayPeriod, new ShiftCategory()));

			var datePeriod = new DateOnlyPeriod(2018, 3, 22, 2018, 3, 24);
			var scheduleDays = getScheduleDays(datePeriod);

			var result = Target.Map(new DateTime(2018, 3, 22), scheduleDays, 0);

			result.Layers.First().StartTimeText.Should().Be.EqualTo("09:15");
		}

		[Test]
		public void ShouldSetEndTimeText()
		{
			setNow();

			var person = setLoggedOnUser();

			var yesterDayPeriod = new DateTimePeriod(2018, 3, 22, 6, 2018, 3, 22, 11).ChangeStartTime(TimeSpan.FromMinutes(55)).ChangeEndTime(TimeSpan.FromMinutes(55));
			var activity1 = new Activity("1").WithId();
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(), activity1, yesterDayPeriod, new ShiftCategory()));

			var datePeriod = new DateOnlyPeriod(2018, 3, 22, 2018, 3, 24);

			var scheduleDays = getScheduleDays(datePeriod);

			var result = Target.Map(new DateTime(2018, 3, 22), scheduleDays, 0);

			result.Layers.First().EndTimeText.Should().Be.EqualTo("12:55");
		}

		[Test]
		public void ShouldSetHours()
		{
			//stockholm +1 
			var hoursAsInts = new List<int>();
			hoursAsInts.AddRange(Enumerable.Range(0, 24));
			hoursAsInts.AddRange(Enumerable.Range(0, 24));
			hoursAsInts.AddRange(Enumerable.Range(0, 24));

			var expected = hoursAsInts.ConvertAll(x => x.ToString(defaultCulture));

			setNow();

			var person = setLoggedOnUser();

			var yesterDayPeriod = new DateTimePeriod(2018, 3, 22, 8, 2018, 3, 22, 17).ChangeStartTime(TimeSpan.FromMinutes(15));
			var activity1 = new Activity("1").WithId();
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(), activity1, yesterDayPeriod, new ShiftCategory()));

			var datePeriod = new DateOnlyPeriod(2018, 3, 22, 2018, 3, 24);
			var scheduleDays = getScheduleDays(datePeriod);

			var result = Target.Map(new DateTime(2018, 3, 22), scheduleDays, 0);

			result.Hours.Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldSetHoursWhenWinterBecomesSummer()
		{
			var hoursAsInts = new List<int> { 0, 1 };
			//02:00 doesn't exist!
			hoursAsInts.AddRange(Enumerable.Range(3, 21));
			hoursAsInts.AddRange(Enumerable.Range(0, 24));
			hoursAsInts.AddRange(Enumerable.Range(0, 24));
			hoursAsInts.Add(0);
			var expected = hoursAsInts.ConvertAll(x => x.ToString(CultureInfo.InvariantCulture));

			Now.Is(new DateTime(2020, 3, 30, 0, 0, 0, DateTimeKind.Utc));

			setLoggedOnUser();

			var datePeriod = new DateOnlyPeriod(2020, 3, 29, 2020, 3, 31);
			var scheduleDays = getScheduleDays(datePeriod);

			var result = Target.Map(new DateTime(2020, 3, 29), scheduleDays, 0);

			result.Hours.Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldSetNumberOfUnreadMessages()
		{
			Now.Is(new DateTime(2020, 3, 30, 0, 0, 0, DateTimeKind.Utc));

			setLoggedOnUser();

			const int numberOfUnreadMessages = 11;
			var datePeriod = new DateOnlyPeriod(2020, 3, 29, 2020, 3, 31);
			var scheduleDays = getScheduleDays(datePeriod);

			var res = Target.Map(new DateTime(2020, 3, 29), scheduleDays, numberOfUnreadMessages);

			res.UnreadMessageCount.Should().Be.EqualTo(11);
		}

		[Test]
		public void ShouldAheadStartMinutesSinceAsmZeroOneHourWhenDstStartWithinThisPeriod()
		{
			Now.Is(new DateTime(2015, 3, 29, 0, 0, 0, DateTimeKind.Utc));
			var person = setLoggedOnUser();

			var layerOneStartTime = new DateTime(2015, 3, 28, 2, 0, 0, DateTimeKind.Utc);
			var layerTwoStartTime = new DateTime(2015, 3, 29, 2, 0, 0, DateTimeKind.Utc);

			var activity1 = new Activity("1").WithId();

			var yesterDayPeriod = new DateTimePeriod(new DateTime(2015, 3, 28, 2, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 3, 28, 2, 0, 0, DateTimeKind.Utc).AddHours(5));
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(),
				activity1,
				yesterDayPeriod, new ShiftCategory()));

			var todayPeriod = new DateTimePeriod(new DateTime(2015, 3, 29, 2, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 3, 29, 2, 0, 0, DateTimeKind.Utc).AddHours(5));
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(),
				activity1,
				todayPeriod, new ShiftCategory()));

			var datePeriod = new DateOnlyPeriod(2015, 3, 28, 2015, 3, 30);
			var scheduleDays = getScheduleDays(datePeriod);

			var asmZore = new DateTime(2015, 3, 28, 0, 0, 0);

			var res = Target.Map(asmZore, scheduleDays, 0);
			var timeZone = person.PermissionInformation.DefaultTimeZone();

			res.Layers.First().StartMinutesSinceAsmZero.Should().Be
				.EqualTo(TimeZoneInfo.ConvertTimeFromUtc(layerOneStartTime, timeZone).Subtract(asmZore).TotalMinutes);
			res.Layers.Second().StartMinutesSinceAsmZero.Should().Be
				.EqualTo(TimeZoneInfo.ConvertTimeFromUtc(layerTwoStartTime, timeZone).Subtract(asmZore).TotalMinutes - 60);
		}


		[Test]
		public void ShouldStartMinutesSinceAsmZeroDelayOneHourWhenDstEndWithinThisPeriod()
		{
			Now.Is(new DateTime(2015, 10, 25, 0, 0, 0, DateTimeKind.Utc));

			var person = setLoggedOnUser();

			var layerOneStartTime = new DateTime(2015, 10, 24, 2, 0, 0, DateTimeKind.Utc);
			var layerTwoStartTime = new DateTime(2015, 10, 25, 2, 0, 0, DateTimeKind.Utc);

			var activity1 = new Activity("1").WithId();

			var yesterDayPeriod = new DateTimePeriod(layerOneStartTime,
				layerOneStartTime.AddHours(5));
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(),
				activity1,
				yesterDayPeriod, new ShiftCategory()));

			var todayPeriod = new DateTimePeriod(layerTwoStartTime,
				layerTwoStartTime.AddHours(5));
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(),
				activity1,
				todayPeriod, new ShiftCategory()));

			var datePeriod = new DateOnlyPeriod(2015, 10, 24, 2015, 10, 26);
			var scheduleDays = getScheduleDays(datePeriod);


			var asmZore = new DateTime(2015, 10, 24, 0, 0, 0);
			var res = Target.Map(asmZore, scheduleDays, 0);

			var timeZone = person.PermissionInformation.DefaultTimeZone();

			res.Layers.First().StartMinutesSinceAsmZero.Should().Be.EqualTo(TimeZoneInfo.ConvertTimeFromUtc(layerOneStartTime, timeZone).Subtract(asmZore).TotalMinutes);
			res.Layers.Second().StartMinutesSinceAsmZero.Should().Be.EqualTo(TimeZoneInfo.ConvertTimeFromUtc(layerTwoStartTime, timeZone).Subtract(asmZore).TotalMinutes + 60);
		}

		private IPerson setLoggedOnUser(TimeZoneInfo timeZoneInfo = null)
		{
			var person = PersonFactory.CreatePersonWithId();
			person.PermissionInformation.SetCulture(defaultCulture);
			person.PermissionInformation.SetUICulture(defaultCulture);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo ?? TimeZoneInfoFactory.StockholmTimeZoneInfo());
			UserTimeZone.Is(timeZoneInfo ?? TimeZoneInfoFactory.StockholmTimeZoneInfo());
			return person;
		}

		private void setNow()
		{
			Now.Is(new DateTime(2018, 3, 23, 0, 0, 0, DateTimeKind.Utc));
		}

		private IEnumerable<IScheduleDay> getScheduleDays(DateOnlyPeriod datePeriod)
		{
			var person = LoggedOnUser.CurrentUser();
			var scheduleDayDictionary =
				ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
					datePeriod, CurrentScenario.Current());

			var scheduleDays = scheduleDayDictionary[person].ScheduledDayCollection(datePeriod);
			return scheduleDays;
		}
	}
}