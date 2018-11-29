using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class ScheduleMatrixValueCalculatorProTest
    {
        private MockRepository _mockRepository;
        private ScheduleMatrixValueCalculatorPro _target;
        private IList<DateOnly> _scheduleDays;
        private SchedulingOptions _schedulingOptions;
        
        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _schedulingOptions = new SchedulingOptions{UseMinimumStaffing = false, UseMaximumStaffing = false};
            _scheduleDays = new List<DateOnly>();
        }

        [Test]
        public void VerifyPeriodValue()
        {
            ISchedulingResultStateHolder stateHolder = createStateHolderForTest(_mockRepository);
            _target = new ScheduleMatrixValueCalculatorPro(_scheduleDays, _schedulingOptions, stateHolder, new UtcTimeZone());

            Assert.IsNotNull(_target);

			Assert.AreEqual(0d, _target.PeriodValue(IterationOperationOption.IntradayOptimization), 0.01d);
            Assert.AreEqual(0d, _target.PeriodValue(IterationOperationOption.WorkShiftOptimization), 0.01d);
        }

        [Test]
        public void VerifyPeriodValue2()
        {
            ISchedulingResultStateHolder stateHolder = createStateHolderForTest(_mockRepository);
            _target = new ScheduleMatrixValueCalculatorPro(_scheduleDays, _schedulingOptions, stateHolder, new UtcTimeZone());

			Assert.AreEqual(0d, _target.PeriodValue(IterationOperationOption.WorkShiftOptimization), 0.01d);
        }

        [Test]
        public void VerifyPeriodValueForDayOffOptimization()
        {
            ISchedulingResultStateHolder stateHolder = createStateHolderForTest(_mockRepository);
            _target = new ScheduleMatrixValueCalculatorPro(_scheduleDays, _schedulingOptions, stateHolder, new UtcTimeZone());

            Assert.AreEqual(0d, _target.PeriodValue(IterationOperationOption.DayOffOptimization), 0.01d);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private ISchedulingResultStateHolder createStateHolderForTest(MockRepository mockRepository)
        {
            
            DateTimePeriod period = new DateTimePeriod(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2008, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            DateTime dateTime = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            ISkill skill = SkillFactory.CreateSkill("Skill");
            ISkillDay skillDay = SkillDayFactory.CreateSkillDay(skill, new DateOnly(dateTime));
            skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new List<ISkillDay> { skillDay }, period.ToDateOnlyPeriod(skill.TimeZone));
           
            SchedulingResultStateHolder stateHolder = SchedulingResultStateHolderFactory.Create(period, skill, new List<ISkillDay>{ skillDay });
            DateOnly day1 = new DateOnly(2008, 1, 1);
            _scheduleDays.Add(day1);

            SkillSkillStaffPeriodExtendedDictionary skillStaffPeriodListForTest =
                createSkillStaffPeriodListForTest(mockRepository, skill, dateTime);

            SkillStaffPeriodHolderFactory.InjectSkillStaffPeriods(
                (SkillStaffPeriodHolder) stateHolder.SkillStaffPeriodHolder,
                skillStaffPeriodListForTest);

            return stateHolder;

        }

        private static SkillSkillStaffPeriodExtendedDictionary createSkillStaffPeriodListForTest(MockRepository mockRepository, ISkill skill, DateTime dateTime)
        {
            DateTimePeriod period1 = new DateTimePeriod(dateTime, dateTime.AddMinutes(15));
            DateTimePeriod period2 = new DateTimePeriod(dateTime.AddMinutes(skill.DefaultResolution), dateTime.AddMinutes(skill.DefaultResolution).AddMinutes(15));
            ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateMockedSkillStaffPeriod(mockRepository, period1, 4, 0, 0, 0, 0);
            ISkillStaffPeriod skillStaffPeriod2 = SkillStaffPeriodFactory.CreateMockedSkillStaffPeriod(mockRepository, period2, 6, 0, 2, 2, 2);
            mockRepository.ReplayAll();
            SkillStaffPeriodDictionary periodDictionary = new SkillStaffPeriodDictionary(skill);
            periodDictionary.Add(skillStaffPeriod1);
            periodDictionary.Add(skillStaffPeriod2);
            SkillSkillStaffPeriodExtendedDictionary dictionary = new SkillSkillStaffPeriodExtendedDictionary();
            dictionary.Add(skill, periodDictionary);
            return dictionary;
        }
    }
}
