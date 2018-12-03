using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Secrets.SkillStaffPeriodDataHolder;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{

    [TestFixture]
    public class SkillStaffPeriodDataHolderTest
    {
        private SkillStaffPeriodDataInfo _target;
        private DateTimePeriod _period;

        [SetUp]
        public void Setup()
        {
            DateTime start = new DateTime(2009,02,02,8,0,0,DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 8, 15, 0, DateTimeKind.Utc);

            _period = new DateTimePeriod(start,end);
            _target = new SkillStaffPeriodDataInfo(5, 10, _period, 5, 10, 0, null);
        }

        [Test]
        public void VerifyNoPublicEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        [Test]
        public void VerifyProperties()
        {
            _target.OriginalDemandInMinutes = 15;
            Assert.AreEqual(15, _target.OriginalDemandInMinutes);
            _target.AssignedResourceInMinutes = 45;
            Assert.AreEqual(45, _target.AssignedResourceInMinutes);
            _target.MaximumPersons = 20;
            Assert.AreEqual(20, _target.MaximumPersons);
            _target.MinimumPersons = 10;
            Assert.AreEqual(10, _target.MinimumPersons);

            Assert.AreEqual(_period, _target.Period);

            _target.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads = -5;
            Assert.AreEqual(-5, _target.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads);
            
        }

        [Test]
        public void VerifyCalculateValueWithMinutes()
        {
            _target = new SkillStaffPeriodDataInfo(150, 120, _period, 0, 0, 0, null);
            var result = _target.PeriodValue(10, false, false);
            Assert.AreEqual(3, result);

            result = _target.PeriodValue(5, false, false);
            Assert.AreEqual(1.5, result);

            _target.OriginalDemandInMinutes = 10;
            _target.AssignedResourceInMinutes = 100;
            result = _target.PeriodValue(10, false, false);
            Assert.AreEqual(45, result, 0.1);

        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyCorrectionFactor()
        {
            DateTime date = new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc);
            DateTimePeriod period1 = new DateTimePeriod(date, date.AddMinutes(15));
            // Not over and not under
            SkillStaffPeriodDataInfo data = new SkillStaffPeriodDataInfo(5, 80, period1, 1, 5, 0, null);

			double corrFactor = SkillStaffPeriodCalculator.GetCorrectionFactor(true, true,data.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads,data.MinimumPersons,data.AssignedResourceInMinutes);
            Assert.AreEqual(0, corrFactor);
            // understaffed
            data = new SkillStaffPeriodDataInfo(5, 80, period1, 2, 5, -3, null);
			corrFactor = SkillStaffPeriodCalculator.GetCorrectionFactor(true, true, data.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads, data.MinimumPersons, data.AssignedResourceInMinutes);

            Assert.AreEqual(150000, corrFactor);

            data = new SkillStaffPeriodDataInfo(5, 0, period1, 2, 5, -3, null);
			corrFactor = SkillStaffPeriodCalculator.GetCorrectionFactor(true, true, data.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads, data.MinimumPersons, data.AssignedResourceInMinutes);

            Assert.AreEqual(300000, corrFactor);

			corrFactor = SkillStaffPeriodCalculator.GetCorrectionFactor(false, true, data.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads, data.MinimumPersons, data.AssignedResourceInMinutes);

            Assert.AreEqual(0, corrFactor);

            // overstaffed
            data = new SkillStaffPeriodDataInfo(5, 80, period1, 2, 5, 2, null);
			corrFactor = SkillStaffPeriodCalculator.GetCorrectionFactor(true, true, data.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads, data.MinimumPersons, data.AssignedResourceInMinutes);

            Assert.AreEqual(-200000, corrFactor);

			corrFactor = SkillStaffPeriodCalculator.GetCorrectionFactor(true, false, data.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads, data.MinimumPersons, data.AssignedResourceInMinutes);

            Assert.AreEqual(0, corrFactor);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyTweakedCurrentDemand()
        {
	        var overstaffing = new Percent(.4);
	        const int priority = 256;

			var tweaked = SkillStaffPeriodCalculator.GetTweakedCurrentDemand(60, 45, overstaffing.Value, priority);
            Assert.AreEqual(-2304, tweaked);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyNewCalculationPeriodValue()
        {
			var valueNew = SkillStaffPeriodCalculator.CalculateWorkShiftPeriodValue(105, -1890, 15);
            Assert.AreEqual(Math.Round(537.8571,4), Math.Round(valueNew,4));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyNewCalculationPeriodValue1()
		{
			var valueNew = SkillStaffPeriodCalculator.CalculateWorkShiftPeriodValue(200, -100, 15);
			Assert.AreEqual(Math.Round(13.875, 4), Math.Round(valueNew, 4));
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyNewCalculationPeriodValue2()
		{
			var valueNew = SkillStaffPeriodCalculator.CalculateWorkShiftPeriodValue(200, 100, 15);
			Assert.AreEqual(Math.Round(-16.125, 4), Math.Round(valueNew, 4));
		}

        [Test]
        public void VerifyNewCalculationPeriodValue3()
		{
			var valueNew = SkillStaffPeriodCalculator.CalculateWorkShiftPeriodValue(0, 1, 15);
			Assert.AreEqual(0, valueNew);
		}

        [Test]
        public void VerifyOriginalDemandReturnsOneMinuteAtLeast()
        {
            _target.OriginalDemandInMinutes = 4;
            Assert.AreEqual(4, _target.OriginalDemandInMinutes);

            _target.OriginalDemandInMinutes = 0;
            Assert.AreEqual(1, _target.OriginalDemandInMinutes);
        }
    }
}
