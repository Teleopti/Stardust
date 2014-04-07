using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ProTest"), TestFixture]
    public class ScheduleMatrixValueCalculatorProTest
    {
        private MockRepository _mockRepository;
        private ScheduleMatrixValueCalculatorPro _target;
        private IList<DateOnly> _scheduleDays;
        private IOptimizationPreferences _optimizerPreferences;
        private IScheduleFairnessCalculator _fairnessCalculator;
        
        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _optimizerPreferences = new OptimizationPreferences();
            _scheduleDays = new List<DateOnly>();
        }

        [Test]
        public void VerifyConstructor()
        {
            SchedulingResultStateHolder stateHolder = new SchedulingResultStateHolder();
            _fairnessCalculator = _mockRepository.StrictMock<IScheduleFairnessCalculator>();
            _target = new ScheduleMatrixValueCalculatorPro(_scheduleDays, _optimizerPreferences, stateHolder, _fairnessCalculator);
            Assert.IsNotNull(_target);
            Assert.AreSame(_scheduleDays, _target.ScheduleDays);
            Assert.AreSame(stateHolder, _target.StateHolder);
            Assert.AreSame(_fairnessCalculator, _target.FairnessCalculator);
        }

        [Test]
        public void VerifyPeriodValue()
        {
            ISchedulingResultStateHolder stateHolder = createStateHolderForTest(_mockRepository);
            _fairnessCalculator = new ScheduleFairnessCalculatorForTest();
            _optimizerPreferences.Advanced.UseIntraIntervalDeviation = true;
            _optimizerPreferences.Extra.FairnessValue = 0.1d;
            _target = new ScheduleMatrixValueCalculatorPro(_scheduleDays, _optimizerPreferences, stateHolder, _fairnessCalculator);

            Assert.IsNotNull(_target);

            Assert.AreEqual(2d, _target.CalculateInitialValue(IterationOperationOption.IntradayOptimization), 0.01d);
            Assert.AreEqual(2.3d, _target.PeriodValue(IterationOperationOption.WorkShiftOptimization), 0.01d);
            stateHolder.Dispose();
        }

        [Test]
        public void VerifyPeriodValue2()
        {
            ISchedulingResultStateHolder stateHolder = createStateHolderForTest(_mockRepository);
            _fairnessCalculator = new ScheduleFairnessCalculatorForTest();
			_optimizerPreferences.Advanced.UseIntraIntervalDeviation = true;
			_optimizerPreferences.Extra.FairnessValue = 0.9d;
            _target = new ScheduleMatrixValueCalculatorPro(_scheduleDays, _optimizerPreferences, stateHolder, _fairnessCalculator);

            Assert.AreEqual(2d, _target.CalculateInitialValue(IterationOperationOption.WorkShiftOptimization), 0.01d);
            Assert.AreEqual(4.7d, _target.PeriodValue(IterationOperationOption.WorkShiftOptimization), 0.01d);
            stateHolder.Dispose();
        }

        [Test]
        public void VerifyPeriodValueForDayOffOptimization()
        {
            ISchedulingResultStateHolder stateHolder = createStateHolderForTest(_mockRepository);
            _fairnessCalculator = new ScheduleFairnessCalculatorForTest();
			_optimizerPreferences.Advanced.UseIntraIntervalDeviation = false;
			_optimizerPreferences.Extra.FairnessValue = 0.9d;

            _target = new ScheduleMatrixValueCalculatorPro(_scheduleDays, _optimizerPreferences, stateHolder, _fairnessCalculator);

            Assert.AreEqual(0d, _target.CalculateInitialValue(IterationOperationOption.WorkShiftOptimization), 0.01d);
            Assert.AreEqual(4.5d, _target.PeriodValue(IterationOperationOption.WorkShiftOptimization), 0.01d);
            stateHolder.Dispose();
        }


        [Test]
        public void VerifyIsConsiderMaximumIntraIntervalStandardDeviation()
        {
			Assert.AreEqual(_optimizerPreferences.Advanced.UseIntraIntervalDeviation, 
                            _target.IsConsiderMaximumIntraIntervalStandardDeviation());
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private ISchedulingResultStateHolder createStateHolderForTest(MockRepository mockRepository)
        {
            
            DateTimePeriod period = new DateTimePeriod(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2008, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            DateTime dateTime = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            ISkill skill = SkillFactory.CreateSkill("Skill");
            ISkillDay skillDay = SkillDayFactory.CreateSkillDay(skill, dateTime);
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
