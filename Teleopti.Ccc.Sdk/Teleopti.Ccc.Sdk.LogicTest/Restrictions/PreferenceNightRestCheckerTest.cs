using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
	[TestFixture]
	public class PreferenceNightRestCheckerTest
	{
		private MockRepository _mocks;
		private PreferenceNightRestChecker _target;
		private INightlyRestFromPersonOnDayExtractor _nightlyRestFromPersonOnDayExtractor;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_nightlyRestFromPersonOnDayExtractor = _mocks.StrictMock<INightlyRestFromPersonOnDayExtractor>();
			_target = new PreferenceNightRestChecker(_nightlyRestFromPersonOnDayExtractor);
		}

		[Test]
		public void ShouldSetWarningOnDtoIfNightRestWillBeBroken()
		{
			var date1 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 3) };

			var day1 = new ValidatedSchedulePartDto { DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes };
			var day2 = new ValidatedSchedulePartDto { DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var day3 = new ValidatedSchedulePartDto { DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var dtos = new List<ValidatedSchedulePartDto>{day1,day2, day3};

			Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date1)).Return(TimeSpan.FromHours(12));
			Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date2)).Return(TimeSpan.FromHours(12));

			_mocks.ReplayAll();
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(dtos);
			Assert.That(dtos[0].ViolatesNightlyRest , Is.True);
			Assert.That(dtos[1].ViolatesNightlyRest , Is.True);
			Assert.That(dtos[2].ViolatesNightlyRest , Is.True);
			_mocks.VerifyAll();
		}
		
		[Test]
		public void ShouldNotSetWarningOnDtoIfNightRestNotWillBeBroken()
		{
			var date1 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 3) };
			var day1 = new ValidatedSchedulePartDto { DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(20).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes };
			var day2 = new ValidatedSchedulePartDto { DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var day3 = new ValidatedSchedulePartDto { DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var dtos = new List<ValidatedSchedulePartDto> { day1, day2, day3 };

			Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date1)).Return(TimeSpan.FromHours(12));
			Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date2)).Return(TimeSpan.FromHours(12));

			_mocks.ReplayAll();
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(dtos);
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.True);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSetWarningOnDtoIfOneDayIsDayOff()
		{
			var date1 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 3) };
			var day1 = new ValidatedSchedulePartDto {  DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes };
			var day2 = new ValidatedSchedulePartDto { HasDayOff = true, DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var day3 = new ValidatedSchedulePartDto {  DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var dtos = new List<ValidatedSchedulePartDto> { day1, day2, day3 };

			Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date1)).Return(TimeSpan.FromHours(12));
			Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date2)).Return(TimeSpan.FromHours(12));

			_mocks.ReplayAll();
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(dtos);
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSetWarningOnDtoIfOneDayIsPreferredDayOff()
		{
			var date1 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 3) };
			var restriction = new PreferenceRestrictionDto {DayOff = new DayOffInfoDto()};
			var day1 = new ValidatedSchedulePartDto { DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes };
			var day2 = new ValidatedSchedulePartDto { PreferenceRestriction = restriction, DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var day3 = new ValidatedSchedulePartDto { DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var dtos = new List<ValidatedSchedulePartDto> { day1, day2, day3 };

			Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date1)).Return(TimeSpan.FromHours(12));
			Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date2)).Return(TimeSpan.FromHours(12));

			_mocks.ReplayAll();
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(dtos);
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			_mocks.VerifyAll();
		}

        [Test]
        public void ShouldNotSetWarningOnDtoIfOneDayIsPreferredAbsence()
        {
			var date1 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 3) };
            var restriction = new PreferenceRestrictionDto { Absence = new AbsenceDto() };
            var day1 = new ValidatedSchedulePartDto { DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes};
            var day2 = new ValidatedSchedulePartDto { PreferenceRestriction = restriction, DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes};
            var day3 = new ValidatedSchedulePartDto { DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes};
            var dtos = new List<ValidatedSchedulePartDto> { day1, day2, day3 };

            Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date1)).Return(TimeSpan.FromHours(12));
            Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date2)).Return(TimeSpan.FromHours(12));

            _mocks.ReplayAll();
            Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
            Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
            Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
            _target.CheckNightlyRest(dtos);
            Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
            Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
            Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
            _mocks.VerifyAll();    
        }

		[Test]
		public void ShouldNotSetWarningOnDtoIfOneDayIsScheduledWithFullDayAbsence()
		{
			var date1 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 2) };
			var date3 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 3) };
			var day1 = new ValidatedSchedulePartDto { DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes };
			var day2 = new ValidatedSchedulePartDto { HasAbsence = true, DateOnly = date2, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var day3 = new ValidatedSchedulePartDto { DateOnly = date3, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(8).TotalMinutes };
			var dtos = new List<ValidatedSchedulePartDto> { day1, day2, day3 };

			Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date1)).Return(TimeSpan.FromHours(12));
			Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date2)).Return(TimeSpan.FromHours(12));

			_mocks.ReplayAll();
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(dtos);
			Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			Assert.That(dtos[2].ViolatesNightlyRest, Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotCheckNightRestIfTheFollowingDayIsUnavailable()
		{
			var date1 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 1) };
			var date2 = new DateOnlyDto { DateTime = new DateOnly(2011, 9, 2) };
			var day1 = new ValidatedSchedulePartDto { DateOnly = date1, MinEndTimeMinute = (int)TimeSpan.FromHours(21).TotalMinutes, MaxStartTimeMinute = (int)TimeSpan.FromHours(15).TotalMinutes };
			var day2 = new ValidatedSchedulePartDto { DateOnly = date2 };
			var dtos = new List<ValidatedSchedulePartDto> { day1, day2 };
			using (_mocks.Record())
			{
				Expect.Call(_nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(date1)).Return(TimeSpan.FromHours(12));
			}
			using (_mocks.Playback())
			{
				_target.CheckNightlyRest(dtos);

				Assert.That(dtos[0].ViolatesNightlyRest, Is.False);
				Assert.That(dtos[1].ViolatesNightlyRest, Is.False);
			}
		}
	}	
}