using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class FullDayAbsenceRequestPeriodUtilTest
    {
        private MockRepository mocks;
	    private IScenario scenario;
        private IGlobalSettingDataRepository globalSettingDataRepository;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            scenario = mocks.DynamicMock<IScenario>();
            globalSettingDataRepository = mocks.StrictMock<IGlobalSettingDataRepository>();
        }

		[Test]
		public void ShouldApplyHiddenGlobalSettingsToFullDayAbsence()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			//create "full day" absence (0:00->23:59)
			var absencePeriodStartTime = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var absencePeriodEndTime = new DateTime(2011, 3, 4, 23, 59, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(absencePeriodStartTime, absencePeriodEndTime);

			// mocked hidden global settings for start/end period when no shift is present..
			var startTimeSpan = new TimeSpan(6, 0, 0);
			var endTimeSpan = new TimeSpan(22, 0, 0);

			var day = mocks.DynamicMock<IScheduleDay>();

			using (mocks.Record())
			{
				Expect.Call(globalSettingDataRepository.FindValueByKey("FullDayAbsenceRequestStartTime", new TimeSpanSetting(new TimeSpan(0, 0, 0))))
					.IgnoreArguments()
				   .Return(new TimeSpanSetting(startTimeSpan));

				Expect.Call(globalSettingDataRepository.FindValueByKey("FullDayAbsenceRequestEndTime", new TimeSpanSetting(new TimeSpan(23, 59, 0))))
				   .IgnoreArguments()
				  .Return(new TimeSpanSetting(endTimeSpan));
			}
			using (mocks.Playback())
			{
				period = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(period, person, day, day,
					globalSettingDataRepository);

				Assert.IsTrue(period.StartDateTime.TimeOfDay == startTimeSpan);
				Assert.IsTrue(period.EndDateTime.TimeOfDay == endTimeSpan);
			}
		}

        [Test]
        public void ShouldApplyHiddenGlobalSettingsToFullDayAbsenceWithDayOff()
        {
            var person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

            //create "full day" absence (0:00->23:59)
            var absencePeriodStartTime = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
            var absencePeriodEndTime = new DateTime(2011, 3, 4, 23, 59, 0, DateTimeKind.Utc);
            var period = new DateTimePeriod(absencePeriodStartTime, absencePeriodEndTime);

            var personAssignment = new PersonAssignment(person, scenario, new DateOnly(period.StartDateTime));

            // mocked hidden global settings for start/end period when no shift is present..
            var startTimeSpan = new TimeSpan(6, 0, 0);
            var endTimeSpan = new TimeSpan(22, 0, 0);

            var day = mocks.DynamicMock<IScheduleDay>();

            using (mocks.Record())
            {
                Expect.Call(day.PersonAssignment()).Return(personAssignment);
                Expect.Call(day.IsScheduled()).Return(true);
                // this is a day off, so should use system settings
                Expect.Call(day.HasDayOff()).Return(true);
                Expect.Call(globalSettingDataRepository.FindValueByKey("FullDayAbsenceRequestStartTime", new TimeSpanSetting(new TimeSpan(0, 0, 0))))
                    .IgnoreArguments()
                   .Return(new TimeSpanSetting(startTimeSpan));

                Expect.Call(globalSettingDataRepository.FindValueByKey("FullDayAbsenceRequestEndTime", new TimeSpanSetting(new TimeSpan(23, 59, 0))))
                   .IgnoreArguments()
                  .Return(new TimeSpanSetting(endTimeSpan));
            }
            using (mocks.Playback())
            {
                period = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(period, person, day, day,
                    globalSettingDataRepository);

                Assert.IsTrue(period.StartDateTime.TimeOfDay == startTimeSpan);
                Assert.IsTrue(period.EndDateTime.TimeOfDay == endTimeSpan);
            }
        }


		[Test]
		public void ShouldApplyShiftTimeToFullDayAbsence()
		{
		    var activityPeriodStart = new DateTime (2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
		    var activityPeriodEnd = new DateTime (2011, 3, 4, 23, 0, 0, DateTimeKind.Utc);

		    var settingStart = new TimeSpan (6, 0, 0);
		    var settingEnd = new TimeSpan (22, 0, 0);

            var activityPeriod = new DateTimePeriod(activityPeriodStart, activityPeriodEnd);
            TestShiftTimeIsAppliedToFullDayAbsence(settingStart, settingEnd, activityPeriod);
		}

        [Test]
        public void ShouldApplyEarlyShiftTimeToFullDayAbsence()
        {
            //start time of shift is after setting start time..
            var activityPeriodStart =  new DateTime(2011, 3, 4, 10, 0, 0, DateTimeKind.Utc);
            var activityPeriodEnd = new DateTime(2011, 3, 4, 23, 0, 0, DateTimeKind.Utc);

            var settingStart = new TimeSpan(0, 0, 0);
            var settingEnd = new TimeSpan(23, 0, 0);

            var activityPeriod = new DateTimePeriod(activityPeriodStart, activityPeriodEnd);
            TestShiftTimeIsAppliedToFullDayAbsence(settingStart, settingEnd, activityPeriod);
        }
        

        [Test]
        public void ShouldApplyLateShiftTimeToFullDayAbsence()
        {
            //end time of shift is earlier than the setting end time
            var activityPeriodStart = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
            var activityPeriodEnd = new DateTime(2011, 3, 4, 22, 0, 0, DateTimeKind.Utc);

            var settingStart = new TimeSpan(0, 0, 0);
            var settingEnd = new TimeSpan(23, 59, 0);

            var activityPeriod = new DateTimePeriod(activityPeriodStart, activityPeriodEnd);
            TestShiftTimeIsAppliedToFullDayAbsence(settingStart, settingEnd, activityPeriod);
        }

        [Test]
        public void ShouldApplyShiftTimeInsideSettingPeriodToFullDayAbsence()
        {
            //end time of shift is earlier than the setting end time AND the start time of the shift is after the setting start date
            var activityPeriodStart = new DateTime(2011, 3, 4, 12, 0, 0, DateTimeKind.Utc);
            var activityPeriodEnd = new DateTime(2011, 3, 4, 20, 0, 0, DateTimeKind.Utc);

            var settingStart = new TimeSpan(10, 0, 0);
            var settingEnd = new TimeSpan(22, 00, 0);

            var activityPeriod = new DateTimePeriod(activityPeriodStart, activityPeriodEnd);
            TestShiftTimeIsAppliedToFullDayAbsence(settingStart, settingEnd, activityPeriod);

        }

        [Test]
        public void ShouldApplyShiftTimeToFullDayAbsenceWhereShiftIsOvernight()
        {
            //the shift starting on the day of the full day absence goes past midnight
            var activityPeriodStart = new DateTime(2011, 3, 4, 22, 0, 0, DateTimeKind.Utc);
            var activityPeriodEnd = new DateTime(2011, 3, 5, 06, 0, 0, DateTimeKind.Utc);

            var settingStart = new TimeSpan(00, 0, 0);
            var settingEnd = new TimeSpan(23, 00, 0);

            var activityPeriod = new DateTimePeriod(activityPeriodStart, activityPeriodEnd);
            TestShiftTimeIsAppliedToFullDayAbsence(settingStart, settingEnd, activityPeriod);

        }


	    [Test]
	    public void EmptyPersonAssignmentShouldWorkTheSameAsEmptyDay()
	    {
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			//create "full day" absence (0:00->23:59)
			var absencePeriodStartTime = new DateTime(2011,3,4,0,0,0,DateTimeKind.Utc);
			var absencePeriodEndTime = new DateTime(2011,3,4,23,59,0,DateTimeKind.Utc);
			var period = new DateTimePeriod(absencePeriodStartTime,absencePeriodEndTime);

			var personAssignment = new PersonAssignment(person,scenario,new DateOnly(2011, 3, 4));		
			var day = mocks.DynamicMock<IScheduleDay>();

			var fullDayAbsenceRequestStartTime = new TimeSpan(0,0,0);
			var fullDayAbsenceRequestEndTime = new TimeSpan(23,59,0);


			using(mocks.Record())
			{
				Expect.Call(day.PersonAssignment()).Return(personAssignment);
				Expect.Call(day.IsScheduled()).Return(true);

				
				Expect.Call(globalSettingDataRepository.FindValueByKey("FullDayAbsenceRequestStartTime",
					new TimeSpanSetting(fullDayAbsenceRequestStartTime)))
					.IgnoreArguments()
					.Return(new TimeSpanSetting(fullDayAbsenceRequestStartTime));

				Expect.Call(globalSettingDataRepository.FindValueByKey("FullDayAbsenceRequestEndTime",
					new TimeSpanSetting(fullDayAbsenceRequestEndTime)))
					.IgnoreArguments()
					.Return(new TimeSpanSetting(fullDayAbsenceRequestEndTime));
			}
			using(mocks.Playback())
			{
				period = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(period,person,day,day,
					globalSettingDataRepository);

				period.StartDateTime.Should().Be.EqualTo(new DateTime(2011,3,4).Add(fullDayAbsenceRequestStartTime));
				period.EndDateTime.Should().Be.EqualTo(new DateTime(2011, 3, 4).Add(fullDayAbsenceRequestEndTime));
			}
		}

        private void TestShiftTimeIsAppliedToFullDayAbsence(TimeSpan fullDaySettingstartTimeSpan, TimeSpan fullDaySettingEndTimeSpan,DateTimePeriod activityDateTimePeriod)
        {
            var person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetDefaultTimeZone (TimeZoneInfo.Utc);
            //create "full day" absence (0:00->23:59)
            var absencePeriodStartTime = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
            var absencePeriodEndTime = new DateTime(2011, 3, 4, 23, 59, 0, DateTimeKind.Utc);
            var period = new DateTimePeriod (absencePeriodStartTime, absencePeriodEndTime);

            var personAssignment = new PersonAssignment(person, scenario, new DateOnly(activityDateTimePeriod.StartDateTime));
            personAssignment.AddActivity(new Activity("Email"), activityDateTimePeriod);

            var day = mocks.DynamicMock<IScheduleDay>();
        
            using (mocks.Record())
            {
                Expect.Call (day.PersonAssignment()).Return (personAssignment);
                Expect.Call (day.IsScheduled()).Return (true);

                Expect.Call (globalSettingDataRepository.FindValueByKey ("FullDayAbsenceRequestStartTime",
                    new TimeSpanSetting(new TimeSpan(0, 0, 0)))) 
                    .IgnoreArguments()
                    .Return (new TimeSpanSetting (fullDaySettingstartTimeSpan));

                Expect.Call (globalSettingDataRepository.FindValueByKey ("FullDayAbsenceRequestEndTime",
                    new TimeSpanSetting(new TimeSpan(23, 59, 0))))
                    .IgnoreArguments()
                    .Return (new TimeSpanSetting (fullDaySettingEndTimeSpan));
            }
            using (mocks.Playback())
            {
                period = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired (period, person, day, day,
                    globalSettingDataRepository);

                Assert.IsTrue(period.StartDateTime == personAssignment.Period.StartDateTime);
                Assert.IsTrue(period.EndDateTime == personAssignment.Period.EndDateTime);
                
            }
        }
    }
}
