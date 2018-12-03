using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
	[TestFixture]
	public class PreferenceNightRestCheckerTest
	{
		[Test]
		public void ShouldSetWarningOnDtoIfNightRestWillBeBroken()
		{
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson(),new DateOnly(2011,9,1));
			person.Period(new DateOnly(2011,9,1)).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero,TimeSpan.FromHours(40),TimeSpan.FromHours(12), TimeSpan.FromHours(36));

			var nightlyRestFromPersonOnDayExtractor = new NightlyRestFromPersonOnDayExtractor(person);
			var target = new PreferenceNightRestChecker(nightlyRestFromPersonOnDayExtractor);

			var date1 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 3) };

			var day1 = new ValidatedSchedulePartDto { DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes };
			var day2 = new ValidatedSchedulePartDto { DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var day3 = new ValidatedSchedulePartDto { DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var dtos = new List<ValidatedSchedulePartDto>{day1,day2, day3};
			
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			target.CheckNightlyRest(dtos);
			Assert.That(dtos[0].ViolatesNightlyRest , Is.True);
			Assert.That(dtos[1].ViolatesNightlyRest , Is.True);
			Assert.That(dtos[2].ViolatesNightlyRest , Is.True);
		}
		
		[Test]
		public void ShouldNotSetWarningOnDtoIfNightRestNotWillBeBroken()
		{
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson(), new DateOnly(2011, 9, 1));
			person.Period(new DateOnly(2011, 9, 1)).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(40), TimeSpan.FromHours(12), TimeSpan.FromHours(36));

			var nightlyRestFromPersonOnDayExtractor = new NightlyRestFromPersonOnDayExtractor(person);
			var target = new PreferenceNightRestChecker(nightlyRestFromPersonOnDayExtractor);

			var date1 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 3) };
			var day1 = new ValidatedSchedulePartDto { DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(20).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes };
			var day2 = new ValidatedSchedulePartDto { DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var day3 = new ValidatedSchedulePartDto { DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var dtos = new List<ValidatedSchedulePartDto> { day1, day2, day3 };
			
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			target.CheckNightlyRest(dtos);
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.True);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.True);
		}

		[Test]
		public void ShouldNotSetWarningOnDtoIfOneDayIsDayOff()
		{
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson(), new DateOnly(2011, 9, 1));
			person.Period(new DateOnly(2011, 9, 1)).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(40), TimeSpan.FromHours(12), TimeSpan.FromHours(36));

			var nightlyRestFromPersonOnDayExtractor = new NightlyRestFromPersonOnDayExtractor(person);
			var target = new PreferenceNightRestChecker(nightlyRestFromPersonOnDayExtractor);

			var date1 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 3) };
			var day1 = new ValidatedSchedulePartDto {  DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes };
			var day2 = new ValidatedSchedulePartDto { HasDayOff = true, DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var day3 = new ValidatedSchedulePartDto {  DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var dtos = new List<ValidatedSchedulePartDto> { day1, day2, day3 };

			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			target.CheckNightlyRest(dtos);
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
		}

		[Test]
		public void ShouldNotSetWarningOnDtoIfOneDayIsPreferredDayOff()
		{
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson(), new DateOnly(2011, 9, 1));
			person.Period(new DateOnly(2011, 9, 1)).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(40), TimeSpan.FromHours(12), TimeSpan.FromHours(36));

			var nightlyRestFromPersonOnDayExtractor = new NightlyRestFromPersonOnDayExtractor(person);
			var target = new PreferenceNightRestChecker(nightlyRestFromPersonOnDayExtractor);

			var date1 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 3) };
			var restriction = new PreferenceRestrictionDto {DayOff = new DayOffInfoDto()};
			var day1 = new ValidatedSchedulePartDto { DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes };
			var day2 = new ValidatedSchedulePartDto { PreferenceRestriction = restriction, DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var day3 = new ValidatedSchedulePartDto { DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var dtos = new List<ValidatedSchedulePartDto> { day1, day2, day3 };

			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			target.CheckNightlyRest(dtos);
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
		}

        [Test]
        public void ShouldNotSetWarningOnDtoIfOneDayIsPreferredAbsence()
        {
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson(), new DateOnly(2011, 9, 1));
			person.Period(new DateOnly(2011, 9, 1)).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(40), TimeSpan.FromHours(12), TimeSpan.FromHours(36));

			var nightlyRestFromPersonOnDayExtractor = new NightlyRestFromPersonOnDayExtractor(person);
			var target = new PreferenceNightRestChecker(nightlyRestFromPersonOnDayExtractor);

			var date1 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 3) };
            var restriction = new PreferenceRestrictionDto { Absence = new AbsenceDto() };
            var day1 = new ValidatedSchedulePartDto { DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes};
            var day2 = new ValidatedSchedulePartDto { PreferenceRestriction = restriction, DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes};
            var day3 = new ValidatedSchedulePartDto { DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes};
            var dtos = new List<ValidatedSchedulePartDto> { day1, day2, day3 };

            Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
            Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
            Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
            target.CheckNightlyRest(dtos);
            Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
            Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
            Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
        }

		[Test]
		public void ShouldNotSetWarningOnDtoIfOneDayIsScheduledWithFullDayAbsence()
		{
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson(), new DateOnly(2011, 9, 1));
			person.Period(new DateOnly(2011, 9, 1)).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(40), TimeSpan.FromHours(12), TimeSpan.FromHours(36));

			var nightlyRestFromPersonOnDayExtractor = new NightlyRestFromPersonOnDayExtractor(person);
			var target = new PreferenceNightRestChecker(nightlyRestFromPersonOnDayExtractor);

			var date1 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateTime(2011, 9, 3) };
			var day1 = new ValidatedSchedulePartDto { DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes };
			var day2 = new ValidatedSchedulePartDto { HasAbsence = true, DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var day3 = new ValidatedSchedulePartDto { DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var dtos = new List<ValidatedSchedulePartDto> { day1, day2, day3 };

			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			target.CheckNightlyRest(dtos);
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
		}

		[Test]
		public void ShouldNotCheckNightRestIfTheFollowingDayIsUnavailable()
		{
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson(),
				new DateOnly(2011, 9, 1));
			person.Period(new DateOnly(2011, 9, 1)).PersonContract.Contract.WorkTimeDirective =
				new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(40), TimeSpan.FromHours(12), TimeSpan.FromHours(36));

			var nightlyRestFromPersonOnDayExtractor = new NightlyRestFromPersonOnDayExtractor(person);
			var target = new PreferenceNightRestChecker(nightlyRestFromPersonOnDayExtractor);

			var date1 = new DateOnlyDto {DateTime = new DateTime(2011, 9, 1)};
			var date2 = new DateOnlyDto {DateTime = new DateTime(2011, 9, 2)};
			var day1 = new ValidatedSchedulePartDto
			{
				DateOnly = date1,
				MinEndTimeMinute = (int) TimeSpan.FromHours(21).TotalMinutes,
				MaxStartTimeMinute = (int) TimeSpan.FromHours(15).TotalMinutes
			};
			var day2 = new ValidatedSchedulePartDto {DateOnly = date2};
			var dtos = new List<ValidatedSchedulePartDto> {day1, day2};

			target.CheckNightlyRest(dtos);

			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
		}
	}	
}