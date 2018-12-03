using System;
using System.Linq;
using System.Reflection;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class SkillStaffPeriodFactory
    {
        public static SkillStaffPeriod CreateSkillStaffPeriod(DateTimePeriod period, ITask task, ServiceAgreement serviceAgreement)
        {
			var staffPeriod = new SkillStaffPeriod(period, task, serviceAgreement);
			staffPeriod.SetSkillDay(SkillDayFactory.CreateSkillDay(SkillFactory.CreateSkill("skill"),new DateOnly(period.StartDateTime)));
	        return staffPeriod;
        }

        public static SkillStaffPeriod CreateSkillStaffPeriod(DateTimePeriod period, ITask task, ServiceAgreement serviceAgreement, ISkillDay parent)
        {
			SkillStaffPeriodForTest staffPeriod = new SkillStaffPeriodForTest(period, task, serviceAgreement);
            staffPeriod.SetParent(parent);
            return staffPeriod;
        }

		public static SkillStaffPeriod CreateSkillStaffPeriod(ISkill skill, DateTime dateTime, int lengthInMinutes, double forecastValue, double resourceValue)
		{
			SkillStaffPeriod skillStaffPeriod =
				new SkillStaffPeriod(new DateTimePeriod(dateTime, dateTime.AddMinutes(lengthInMinutes)), new Task(),
									 ServiceAgreement.DefaultValues());
			skillStaffPeriod.SetSkillDay(SkillDayFactory.CreateSkillDay(SkillFactory.CreateSkill("skill"),new DateOnly(dateTime)));
			skillStaffPeriod.CalculateStaff();
			skillStaffPeriod.IsAvailable = true;

			skillStaffPeriod = InjectForecastedIncomingDemand(skillStaffPeriod, forecastValue);
			skillStaffPeriod = InjectForecastedDistributedDemand(skillStaffPeriod, forecastValue);
			skillStaffPeriod = InjectCalculatedResource(skillStaffPeriod, resourceValue);

			return skillStaffPeriod;
		}

        public static SkillStaffPeriod CreateSkillStaffPeriod(ISkill skill, DateTime dateTime, double forecastValue, double resourceValue)
        {
            SkillStaffPeriod skillStaffPeriod =
                new SkillStaffPeriod(new DateTimePeriod(dateTime, dateTime.AddMinutes(15)), new Task(),
                                     ServiceAgreement.DefaultValues());
			skillStaffPeriod.SetSkillDay(SkillDayFactory.CreateSkillDay(skill, new DateOnly(dateTime)));
            skillStaffPeriod.CalculateStaff();
            skillStaffPeriod.IsAvailable = true;

            skillStaffPeriod = InjectForecastedIncomingDemand(skillStaffPeriod, forecastValue);
            skillStaffPeriod = InjectForecastedDistributedDemand(skillStaffPeriod, forecastValue);
            skillStaffPeriod = InjectCalculatedResource(skillStaffPeriod, resourceValue);

            return skillStaffPeriod;
        }

        public static ISkillStaffPeriod CreateMockedSkillStaffPeriod(
            MockRepository mockRepository,
            DateTimePeriod period)
        {
            ISkillStaffPeriod skillStaffPeriod = mockRepository.StrictMock<ISkillStaffPeriod>();
            Expect.Call(skillStaffPeriod.IsAvailable).Return(true).Repeat.Any();
            Expect.Call(skillStaffPeriod.DateTimePeriod).Return(period).Repeat.Any();
            Expect.Call(skillStaffPeriod.Period).Return(period).Repeat.Any();
            return skillStaffPeriod;
        }

        public static ISkillStaffPeriod CreateMockedSkillStaffPeriod(
            MockRepository mockRepository, 
            DateTimePeriod period, 
            double absoluteDifference, 
            double relativeDifference, 
            double intraIntervalDeviation,
            double forecastHours,
            double scheduledHours)
        {
            ISkillStaffPeriod skillStaffPeriod = CreateMockedSkillStaffPeriod(mockRepository, period);
            InjectMockedAbsoluteDifference(skillStaffPeriod, absoluteDifference);
            InjectMockedRelativeDifference(skillStaffPeriod, relativeDifference);
            InjectMockedIntraIntervalDeviation(skillStaffPeriod, intraIntervalDeviation);
            InjectMockedFStaffHours(skillStaffPeriod, forecastHours);
            InjectMockedScheduledHours(skillStaffPeriod, scheduledHours);
            return skillStaffPeriod;
        }

        public static ISkillStaffPeriod CreateSkillStaffPeriod()
        {
            DateTimePeriod period = new DateTimePeriod();

            Task task = new Task(200, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10));

            ServiceAgreement sa = new ServiceAgreement(new ServiceLevel(new Percent(0.9), 30), new Percent(0.7), new Percent(0.95));

            return CreateSkillStaffPeriod(period, task, sa);
        }

        public static SkillStaffPeriod InjectForecastedIncomingDemand(SkillStaffPeriod skillStaffPeriod, double value)
        {
            ISkillStaff tLayer = skillStaffPeriod.Payload;
            typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(tLayer, value);
            return skillStaffPeriod;
        }

                /// <summary>
        /// Sets the forecasted incoming demand property.
        /// </summary>
        /// <param name="skillStaffPeriod">The skill staff period.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// forecastedDistributedDemand
        public static SkillStaffPeriod InjectForecastedDistributedDemand(SkillStaffPeriod skillStaffPeriod, double value)
        {
            ISkillStaffSegmentPeriod skillStaffSegmentPeriod = skillStaffPeriod.SegmentInThisCollection.First();
            ISkillStaffSegment skillStaffSegment = skillStaffSegmentPeriod.Payload;
            typeof(SkillStaffSegment).GetField("_forecastedDistributedDemand", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(skillStaffSegment, value);

            return skillStaffPeriod;
        }

        /// <summary>
        /// Sets the CalculatedResource property.
        /// </summary>
        /// <param name="skillStaffPeriod">The skill staff period.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static SkillStaffPeriod InjectCalculatedResource(SkillStaffPeriod skillStaffPeriod, double value)
        {
            ISkillStaff tLayer = skillStaffPeriod.Payload;
            typeof(SkillStaff).GetField("_calculatedResource", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(tLayer, value);
            return skillStaffPeriod;
        }

        public static SkillStaffPeriod InjectEstimatedServiceLevel(SkillStaffPeriod skillStaffPeriod, Percent value)
        {
            ISkillStaffPeriod tLayer = skillStaffPeriod;
            typeof(SkillStaffPeriod).GetField("_estimatedServiceLevel", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(tLayer, value);
            return skillStaffPeriod;
        }

        public static void InjectMockedAbsoluteDifference(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.AbsoluteDifference).Return(value).Repeat.Any();
        }

        public static void InjectMockedRelativeDifference(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.RelativeDifference).Return(value).Repeat.Any();
        }

        public static void InjectMockedIntraIntervalDeviation(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.IntraIntervalDeviation).Return(value).Repeat.Any();
        }

        public static void InjectMockedFStaffHours(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.FStaffTime()).Return(TimeSpan.FromHours(value)).Repeat.Any();
            Expect.Call(skillStaffPeriod.FStaffHours()).Return(value).Repeat.Any();
        }

        public static void InjectMockedForecastedIncomingDemand(ISkillStaffPeriod skillStaffPeriod, TimeSpan value)
        {
            Expect.Call(skillStaffPeriod.ForecastedIncomingDemand()).Return(value).Repeat.Any();
        }

        public static void InjectMockedScheduledAgentsIncoming(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.ScheduledAgentsIncoming).Return(value).Repeat.Any();
        }

        public static void InjectMockedScheduledHours(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.CalculatedResource).Return(TimeSpan.FromHours(value).TotalHours).Repeat.Any();
            Expect.Call(skillStaffPeriod.ScheduledHours()).Return(value).Repeat.Any();
        }

        public static void InjectMockedAbsoluteDifferenceMinStaffBoosted(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.AbsoluteDifferenceMinStaffBoosted()).Return(value).Repeat.Any();
        }

        public static void InjectMockedAbsoluteDifferenceMaxStaffBoosted(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.AbsoluteDifferenceMaxStaffBoosted()).Return(value).Repeat.Any();
        }

        public static void InjectMockedAbsoluteDifferenceMinMaxStaffBoosted(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.AbsoluteDifferenceBoosted()).Return(value).Repeat.Any();
        }

        public static void InjectMockedRelativeDifferenceMinStaffBoosted(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.RelativeDifferenceMinStaffBoosted()).Return(value).Repeat.Any();
        }

        public static void InjectMockedRelativeDifferenceMaxStaffBoosted(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.RelativeDifferenceMaxStaffBoosted()).Return(value).Repeat.Any();
        }

        public static void InjectMockedRelativeDifferenceMinMaxStaffBoosted(ISkillStaffPeriod skillStaffPeriod, double value)
        {
            Expect.Call(skillStaffPeriod.RelativeDifferenceBoosted()).Return(value).Repeat.Any();
        }

        private class SkillStaffPeriodForTest : SkillStaffPeriod
        {
            public SkillStaffPeriodForTest(DateTimePeriod period, ITask taskData, ServiceAgreement serviceAgreementData)
                : base(period, taskData, serviceAgreementData)
            {
            }

            public void SetParent(ISkillDay skillDay)
            {
                base.SetSkillDay(skillDay);
            }
        }
    }
}
