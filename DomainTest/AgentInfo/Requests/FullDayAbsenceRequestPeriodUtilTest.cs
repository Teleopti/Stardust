using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Remoting.Messaging;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
			person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));

			//create "full day" absence (0:00->23:59)
			var absencePeriodStartTime = DateTime.SpecifyKind(new DateTime(2011, 3, 4, 0, 0, 0), DateTimeKind.Utc);
			var absencePeriodEndTime = DateTime.SpecifyKind(new DateTime(2011, 3, 4, 23, 59, 0), DateTimeKind.Utc);
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
		public void ShouldApplyShiftTimeToFullDayAbsence()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
			//create "full day" absence (0:00->23:59)
			var absencePeriodStartTime = DateTime.SpecifyKind(new DateTime(2011, 3, 4, 0, 0, 0), DateTimeKind.Utc);
			var absencePeriodEndTime = DateTime.SpecifyKind(new DateTime(2011, 3, 4, 23, 59, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(absencePeriodStartTime, absencePeriodEndTime);

			var personAssignment = new PersonAssignment(person, scenario, new DateOnly(period.StartDateTime));

			var day = mocks.DynamicMock<IScheduleDay>();
			// mocked hidden global settings for start/end period when no shift is present..
			var startTimeSpan = new TimeSpan(6, 0, 0);
			var endTimeSpan = new TimeSpan(22, 0, 0);

			using (mocks.Record())
			{
				Expect.Call(day.PersonAssignment()).Return(personAssignment);
				Expect.Call(day.IsScheduled()).Return(true);

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

				Assert.IsTrue(period.StartDateTime.TimeOfDay == personAssignment.Period.StartDateTime.TimeOfDay);
				Assert.IsTrue(period.EndDateTime.TimeOfDay == personAssignment.Period.EndDateTime.TimeOfDay);
			}
		}
    }
}
