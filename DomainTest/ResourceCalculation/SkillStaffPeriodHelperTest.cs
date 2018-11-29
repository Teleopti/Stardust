using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class SkillStaffPeriodHelperTest
    {
        private IEnumerable<ISkillStaffPeriod> _skillStaffPeriods;
        private ISkillStaffPeriod _skillStaffPeriod1;
        private ISkillStaffPeriod _skillStaffPeriod2;
        private ISkillStaffPeriod _skillStaffPeriod3;
        private MockRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();

            DateTime dateTimeBefore = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime dateTimeAfter = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(15);

            DateTimePeriod period1 = new DateTimePeriod(dateTimeBefore, dateTimeAfter);
            DateTimePeriod period2 = new DateTimePeriod(dateTimeBefore, dateTimeAfter);
            DateTimePeriod period3 = new DateTimePeriod(dateTimeBefore, dateTimeAfter);

            _skillStaffPeriod1 = SkillStaffPeriodFactory.CreateMockedSkillStaffPeriod(_mockRepository, period1);
            _skillStaffPeriod2 = SkillStaffPeriodFactory.CreateMockedSkillStaffPeriod(_mockRepository, period2);
            _skillStaffPeriod3 = SkillStaffPeriodFactory.CreateMockedSkillStaffPeriod(_mockRepository, period3);

        }

        [Test]
        public void VerifySkillStaffPeriodsAbsoluteDifferenceHours()
        {
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifference(_skillStaffPeriod1, 4);
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifference(_skillStaffPeriod2, -4);
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifference(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            IList<double> result = SkillStaffPeriodHelper.SkillStaffPeriodsAbsoluteDifferenceHours(_skillStaffPeriods, false, false);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(-1, result[1]);
            Assert.AreEqual(0, result[2]);

        }

        [Test]
        public void VerifySkillStaffPeriodsMinStaffBoostedAbsoluteDifferenceHours()
        {
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifferenceMinStaffBoosted(_skillStaffPeriod1, 4);
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifferenceMinStaffBoosted(_skillStaffPeriod2, -4);
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifferenceMinStaffBoosted(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            IList<double> result = SkillStaffPeriodHelper.SkillStaffPeriodsAbsoluteDifferenceHours(_skillStaffPeriods, true, false);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(-1, result[1]);
            Assert.AreEqual(0, result[2]);
        }

        [Test]
        public void VerifySkillStaffPeriodsMaxStaffBoostedAbsoluteDifferenceHours()
        {
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifferenceMaxStaffBoosted(_skillStaffPeriod1, 4);
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifferenceMaxStaffBoosted(_skillStaffPeriod2, -4);
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifferenceMaxStaffBoosted(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            IList<double> result = SkillStaffPeriodHelper.SkillStaffPeriodsAbsoluteDifferenceHours(_skillStaffPeriods, false, true);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(-1, result[1]);
            Assert.AreEqual(0, result[2]);
        }

        [Test]
        public void VerifySkillStaffPeriodsMinMaxStaffBoostedAbsoluteDifferenceHours()
        {
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifferenceMinMaxStaffBoosted(_skillStaffPeriod1, 4);
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifferenceMinMaxStaffBoosted(_skillStaffPeriod2, -4);
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifferenceMinMaxStaffBoosted(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            IList<double> result = SkillStaffPeriodHelper.SkillStaffPeriodsAbsoluteDifferenceHours(_skillStaffPeriods, true, true);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(-1, result[1]);
            Assert.AreEqual(0, result[2]);
        }

        [Test]
        public void VerifySkillStaffPeriodsRelativeDifferenceHours()
        {
            SkillStaffPeriodFactory.InjectMockedRelativeDifference(_skillStaffPeriod1, 4);
            SkillStaffPeriodFactory.InjectMockedRelativeDifference(_skillStaffPeriod2, -4);
            SkillStaffPeriodFactory.InjectMockedRelativeDifference(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            IList<double> result = SkillStaffPeriodHelper.SkillStaffPeriodsRelativeDifferenceHours(_skillStaffPeriods, false, false);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(-1, result[1]);
            Assert.AreEqual(0, result[2]);
        }

        [Test]
        public void VerifySkillStaffPeriodsMinStaffBoostedRelativeDifferenceHours()
        {
            SkillStaffPeriodFactory.InjectMockedRelativeDifferenceMinStaffBoosted(_skillStaffPeriod1, 4);
            SkillStaffPeriodFactory.InjectMockedRelativeDifferenceMinStaffBoosted(_skillStaffPeriod2, -4);
            SkillStaffPeriodFactory.InjectMockedRelativeDifferenceMinStaffBoosted(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            IList<double> result = SkillStaffPeriodHelper.SkillStaffPeriodsRelativeDifferenceHours(_skillStaffPeriods, true, false);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(-1, result[1]);
            Assert.AreEqual(0, result[2]);
        }

        [Test]
        public void VerifySkillStaffPeriodsMaxStaffBoostedRelativeDifferenceHours()
        {
            SkillStaffPeriodFactory.InjectMockedRelativeDifferenceMaxStaffBoosted(_skillStaffPeriod1, 4);
            SkillStaffPeriodFactory.InjectMockedRelativeDifferenceMaxStaffBoosted(_skillStaffPeriod2, -4);
            SkillStaffPeriodFactory.InjectMockedRelativeDifferenceMaxStaffBoosted(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            IList<double> result = SkillStaffPeriodHelper.SkillStaffPeriodsRelativeDifferenceHours(_skillStaffPeriods, false, true);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(-1, result[1]);
            Assert.AreEqual(0, result[2]);
        }

        [Test]
        public void VerifySkillStaffPeriodsMinMaxStaffBoostedRelativeDifferenceHours()
        {
            SkillStaffPeriodFactory.InjectMockedRelativeDifferenceMinMaxStaffBoosted(_skillStaffPeriod1, 4);
            SkillStaffPeriodFactory.InjectMockedRelativeDifferenceMinMaxStaffBoosted(_skillStaffPeriod2, -4);
            SkillStaffPeriodFactory.InjectMockedRelativeDifferenceMinMaxStaffBoosted(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            IList<double> result = SkillStaffPeriodHelper.SkillStaffPeriodsRelativeDifferenceHours(_skillStaffPeriods, true, true);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(-1, result[1]);
            Assert.AreEqual(0, result[2]);
        }

        [Test]
        public void VerifyCalculateAbsoluteRootMeanSquare()
        {
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifference(_skillStaffPeriod1, 4);
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifference(_skillStaffPeriod2, -4);
            SkillStaffPeriodFactory.InjectMockedAbsoluteDifference(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            double? result = SkillStaffPeriodHelper.SkillDayRootMeanSquare(_skillStaffPeriods);

            // RMS of {1, -1, 0} >> SQRT(2/3)
            Assert.AreEqual(Math.Sqrt(2d/3d), result.Value, 0.001);
        }

        [Test]
        public void VerifyRelativeDifferenceWithNoForecastNoScheduled ()
        {
            SkillStaffPeriodFactory.InjectMockedFStaffHours(_skillStaffPeriod1, 0);
            SkillStaffPeriodFactory.InjectMockedFStaffHours(_skillStaffPeriod2, 0);
            SkillStaffPeriodFactory.InjectMockedFStaffHours(_skillStaffPeriod3, 0);

            SkillStaffPeriodFactory.InjectMockedScheduledHours(_skillStaffPeriod1, 0);
            SkillStaffPeriodFactory.InjectMockedScheduledHours(_skillStaffPeriod2, 0);
            SkillStaffPeriodFactory.InjectMockedScheduledHours(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            double? result = SkillStaffPeriodHelper.RelativeDifference(_skillStaffPeriods);

            Assert.IsNull(result);
        }

        [Test]
        public void VerifyRelativeDifferenceForDisplayWithNoForecastNoScheduled()
        {
            SkillStaffPeriodFactory.InjectMockedFStaffHours(_skillStaffPeriod1, 0);
            SkillStaffPeriodFactory.InjectMockedFStaffHours(_skillStaffPeriod2, 0);
            SkillStaffPeriodFactory.InjectMockedFStaffHours(_skillStaffPeriod3, 0);

            SkillStaffPeriodFactory.InjectMockedScheduledHours(_skillStaffPeriod1, 0);
            SkillStaffPeriodFactory.InjectMockedScheduledHours(_skillStaffPeriod2, 0);
            SkillStaffPeriodFactory.InjectMockedScheduledHours(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            double? result = SkillStaffPeriodHelper.RelativeDifferenceForDisplay(_skillStaffPeriods);

            Assert.IsNull(result);
        }

        [Test]
        public void VerifyRelativeDifferenceIncomingWithNoForecastNoScheduled()
        {
            SkillStaffPeriodFactory.InjectMockedForecastedIncomingDemand(_skillStaffPeriod1, new TimeSpan(0));
            SkillStaffPeriodFactory.InjectMockedForecastedIncomingDemand(_skillStaffPeriod2, new TimeSpan(0));
            SkillStaffPeriodFactory.InjectMockedForecastedIncomingDemand(_skillStaffPeriod3, new TimeSpan(0));

            SkillStaffPeriodFactory.InjectMockedScheduledAgentsIncoming(_skillStaffPeriod1, 0);
            SkillStaffPeriodFactory.InjectMockedScheduledAgentsIncoming(_skillStaffPeriod2, 0);
            SkillStaffPeriodFactory.InjectMockedScheduledAgentsIncoming(_skillStaffPeriod3, 0);

            _mockRepository.ReplayAll();

            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

            double? result = SkillStaffPeriodHelper.RelativeDifferenceIncoming(_skillStaffPeriods);

            Assert.IsNull(result);
        }

		[Test]
		public void VerifyMaxUsedSeats()
		{
			_skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod();
			_skillStaffPeriod1.Payload.CalculatedUsedSeats = 1;
			_skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod();
			_skillStaffPeriod2.Payload.CalculatedUsedSeats = 9;
			_skillStaffPeriod3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod();
			_skillStaffPeriod3.Payload.CalculatedUsedSeats = 5;

			_skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };

			double? result = SkillStaffPeriodHelper.MaxUsedSeats(_skillStaffPeriods);

			Assert.AreEqual(9, result.Value);
		}

		[Test]
		public void VerifyCalculationsForSpecificPeriod()
		{
			var periods = new List<IEnumerable<ISkillStaffPeriod>>();
			var res = SkillStaffPeriodHelper.SkillPeriodGridSmoothness(periods);
			Assert.IsNull(res);

			Expect.Call(_skillStaffPeriod1.FStaffTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
			Expect.Call(_skillStaffPeriod2.FStaffTime()).Return(TimeSpan.FromHours(16)).Repeat.Any();
			Expect.Call(_skillStaffPeriod3.FStaffTime()).Return(TimeSpan.FromHours(24)).Repeat.Any();

			Expect.Call(_skillStaffPeriod1.CalculatedResource).Return(0).Repeat.Any();
			Expect.Call(_skillStaffPeriod2.CalculatedResource).Return(0).Repeat.Any();
			Expect.Call(_skillStaffPeriod3.CalculatedResource).Return(96).Repeat.Any();

			_mockRepository.ReplayAll();
			var skillStaffPeriods1 = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2 };
			var skillStaffPeriods2 = new List<ISkillStaffPeriod> {  _skillStaffPeriod3 };
			var skillStaffPeriods = new[] {skillStaffPeriods1, skillStaffPeriods2};
			
			res = SkillStaffPeriodHelper.SkillPeriodGridSmoothness(skillStaffPeriods);
			Assert.That(res.Value, Is.EqualTo(0.5));
		}

    }
}
