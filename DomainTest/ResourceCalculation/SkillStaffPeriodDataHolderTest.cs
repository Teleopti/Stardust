﻿using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{

    [TestFixture]
    public class SkillStaffPeriodDataHolderTest
    {
        private SkillStaffPeriodDataHolder _target;
        private DateTimePeriod _period;

        [SetUp]
        public void Setup()
        {
            DateTime start = new DateTime(2009,02,02,8,0,0,DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 8, 15, 0, DateTimeKind.Utc);

            _period = new DateTimePeriod(start,end);
            _target = new SkillStaffPeriodDataHolder(5, 10, _period, 5, 10, 0, null);
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
            _target = new SkillStaffPeriodDataHolder(150, 120, _period, 0, 0, 0, null);
            var result = _target.PeriodValue(10, false, false);
            Assert.AreEqual(1, result);

            result = _target.PeriodValue(5, false, false);
            Assert.AreEqual(.5, result);

            result = _target.PeriodValue(10, false, false);
            Assert.AreEqual(1, result);

            _target.OriginalDemandInMinutes = 10;
            _target.AssignedResourceInMinutes = 100;
            result = _target.PeriodValue(10, false, false);
            Assert.AreEqual(15, result, 0.1);

        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyCorrectionFactor()
        {
            DateTime date = new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc);
            DateTimePeriod period1 = new DateTimePeriod(date, date.AddMinutes(15));
            // Not over and not under
            SkillStaffPeriodDataHolder dataHolder = new SkillStaffPeriodDataHolder(5, 80, period1, 1, 5, 0, null);

            double corrFactor = dataHolder.GetCorrectionFactor( true, true);
            Assert.AreEqual(0, corrFactor);
            // understaffed
            dataHolder = new SkillStaffPeriodDataHolder(5, 80, period1, 2, 5, -3, null);
            corrFactor = dataHolder.GetCorrectionFactor( true, true);
            Assert.AreEqual(150000, corrFactor);

            dataHolder = new SkillStaffPeriodDataHolder(5, 0, period1, 2, 5, -3, null);
            corrFactor = dataHolder.GetCorrectionFactor( true, true);
            Assert.AreEqual(300000, corrFactor);

            corrFactor = dataHolder.GetCorrectionFactor( false, true);
            Assert.AreEqual(0, corrFactor);

            // overstaffed
            dataHolder = new SkillStaffPeriodDataHolder(5, 80, period1, 2, 5, 2, null);
            corrFactor = dataHolder.GetCorrectionFactor( true, true);
            Assert.AreEqual(-200000, corrFactor);

            corrFactor = dataHolder.GetCorrectionFactor( true, false);
            Assert.AreEqual(0, corrFactor);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyCalculateWeightedRelativeDemand()
        {
            double res = SkillStaffPeriodDataHolder.CalculateWeightedRelativeDemand(0, 50);
            Assert.AreEqual(-2500, res);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyTweakedCurrentDemand()
        {
            DateTime date = new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc);
            DateTimePeriod period1 = new DateTimePeriod(date, date.AddMinutes(15));
            
            SkillStaffPeriodDataHolder dataHolder = new SkillStaffPeriodDataHolder(60, 45, period1, 1, 5, 0, null,new Percent(.4),256);

            var tweaked = dataHolder.GetTweakedCurrentDemand(60, 45);
            Assert.AreEqual(-2304, tweaked);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyNewCalculationPeriodValue()
        {
            var valueNew = SkillStaffPeriodDataHolder.CalculateWorkShiftPeriodValue(105, -1890, 15);
            Assert.AreEqual(Math.Round(537.8571,4), Math.Round(valueNew,4));
        }

		public void VerifyNewCalculationPeriodValue1()
		{
			var valueNew = SkillStaffPeriodDataHolder.CalculateWorkShiftPeriodValue(200, -100, 15);
			Assert.AreEqual(Math.Round(13.875, 4), Math.Round(valueNew, 4));
		}

		public void VerifyNewCalculationPeriodValue2()
		{
			var valueNew = SkillStaffPeriodDataHolder.CalculateWorkShiftPeriodValue(200, 100, 15);
			Assert.AreEqual(Math.Round(-16.125, 4), Math.Round(valueNew, 4));
		}

		public void VerifyNewCalculationPeriodValue3()
		{
			var valueNew = SkillStaffPeriodDataHolder.CalculateWorkShiftPeriodValue(0, 1, 15);
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
