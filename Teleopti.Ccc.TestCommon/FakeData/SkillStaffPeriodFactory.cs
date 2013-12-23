using System;
using System.Reflection;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class SkillStaffPeriodFactory
    {
        /// <summary>
        /// Creates a skill staff period.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="task">The task.</param>
        /// <param name="serviceAgreement">The service agreement.</param>
        /// <returns></returns>
        public static SkillStaffPeriod CreateSkillStaffPeriod(DateTimePeriod period, ITask task, ServiceAgreement serviceAgreement)
        {
			var staffPeriod = new SkillStaffPeriod(period, task, serviceAgreement, new StaffingCalculatorService());
			staffPeriod.SetSkillDay(SkillDayFactory.CreateSkillDay(SkillFactory.CreateSkill("skill"),period.StartDateTime));
	        return staffPeriod;
        }

        public static SkillStaffPeriod CreateSkillStaffPeriod(DateTimePeriod period, ITask task, ServiceAgreement serviceAgreement, ISkillDay parent)
        {
            SkillStaffPeriodForTest staffPeriod = new SkillStaffPeriodForTest(period, task, serviceAgreement, new StaffingCalculatorService());
            staffPeriod.SetParent(parent);
            return staffPeriod;
        }

		public static SkillStaffPeriod CreateSkillStaffPeriod(ISkill skill, DateTime dateTime, int lengthInMinutes, double forecastValue, double resourceValue)
		{
			SkillStaffPeriod skillStaffPeriod =
				new SkillStaffPeriod(new DateTimePeriod(dateTime, dateTime.AddMinutes(lengthInMinutes)), new Task(),
									 ServiceAgreement.DefaultValues(), skill.SkillType.StaffingCalculatorService);
			skillStaffPeriod.SetSkillDay(SkillDayFactory.CreateSkillDay(SkillFactory.CreateSkill("skill"),dateTime));
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
                                     ServiceAgreement.DefaultValues(), skill.SkillType.StaffingCalculatorService);
			skillStaffPeriod.SetSkillDay(SkillDayFactory.CreateSkillDay(skill, dateTime));
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


        /// <summary>
        /// Creates a skill staff period.
        /// </summary>
        /// <returns></returns>
        public static ISkillStaffPeriod CreateSkillStaffPeriod()
        {
            DateTimePeriod period = new DateTimePeriod();

            Task task = new Task(200, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10));

            ServiceAgreement sa =
                new ServiceAgreement(new ServiceLevel(new Percent(0.9), 30),
                                     new Percent(0.7), new Percent(0.95));

            return CreateSkillStaffPeriod(period, task, sa);
        }

        /// <summary>
        /// Sets the forecasted incoming demand property.
        /// </summary>
        /// <param name="skillStaffPeriod">The skill staff period.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
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
            ISkillStaffSegmentPeriod skillStaffSegmentPeriod = skillStaffPeriod.SegmentInThisCollection[0];
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
            public SkillStaffPeriodForTest(DateTimePeriod period, ITask taskData, ServiceAgreement serviceAgreementData, IStaffingCalculatorService staffingCalculatorService)
                : base(period, taskData, serviceAgreementData, staffingCalculatorService)
            {
            }

            public void SetParent(ISkillDay skillDay)
            {
                base.SetSkillDay(skillDay);
            }
        }
    }
}
