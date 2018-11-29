using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class PeriodDistributionTest
    {
        private DateTime _start;
        private PeriodDistribution _target;
        private IActivity _activity;
        DateTimePeriod _period;
        private IVisualLayerFactory layerFactory;
        private double _demandedTraff;
        private ISkillStaffPeriod _skillStaffPeriod;

        [SetUp]
        public void Setup()
        {
            layerFactory = new VisualLayerFactory();
            _start = new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc);
            
            _activity = ActivityFactory.CreateActivity("fån");
            _period = new DateTimePeriod(_start, _start.AddMinutes(15));
            _demandedTraff = 0.5;
            _skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod();
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, _period, 5, _demandedTraff);
        }

        [Test]
        public void VerifySetup()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(3, _target.GetSplitPeriodValues().Length);
        }

        [Test]
        public void VerifyDeviation()
        {
            Assert.AreEqual(0,_target.CalculateStandardDeviation());
        }

        [Test]
        public void VerifySplitWhenEven()
        {
            DateTimePeriod period = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(75));
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, period, 5, _demandedTraff);
            
	        var resourceContainer = MockRepository.GenerateMock<IResourceCalculationDataContainerWithSingleOperation>();
	        resourceContainer.Stub(x=>x.IntraIntervalResources(_skillStaffPeriod.SkillDay.Skill, period)).Return(new[]{period});

            _target.ProcessLayers(resourceContainer, _skillStaffPeriod.SkillDay.Skill);
            Assert.AreEqual(5, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(15,_target.PeriodDetailsSum);

            _target.ProcessLayers(resourceContainer, _skillStaffPeriod.SkillDay.Skill);
            Assert.AreEqual(5, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(15, _target.PeriodDetailsSum);
           
        }

        [Test]
        public void VerifySplitWhenLayersShorter()
        {
            DateTimePeriod period = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(75));
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, period, 5, _demandedTraff);

			var resourceContainer = MockRepository.GenerateMock<IResourceCalculationDataContainerWithSingleOperation>();
			resourceContainer.Stub(x => x.IntraIntervalResources(_skillStaffPeriod.SkillDay.Skill, period)).Return(new[] { new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(67)) });

			_target.ProcessLayers(resourceContainer, _skillStaffPeriod.SkillDay.Skill);

            Assert.AreEqual(5, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(2, _target.GetSplitPeriodValues()[1]);
            Assert.AreEqual(0, _target.GetSplitPeriodValues()[2]);
            Assert.AreEqual(7, _target.PeriodDetailsSum);

            double expected = Domain.Calculation.Variances.StandardDeviation(new[] {1d, -0.19d, -1d});
            Assert.AreEqual(expected, _target.CalculateStandardDeviation(), 0.01);

            Assert.AreEqual(2.33, _target.PeriodDetailAverage, 0.01);
        }

		[Test]
		public void VerifyWhenSequenceContainsNonNumericValuesOnly()
		{
			double expected = Domain.Calculation.Variances.StandardDeviation(new[] { double.NaN });
			Assert.AreEqual(0, expected);
		}

        [Test]
        public void VerifyWhenZero()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				_target = new PeriodDistribution(_skillStaffPeriod, _activity, _period, 0, _demandedTraff);
			});
        }

        [Test]
        public void VerifyAverage()
        {
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, _period, 5, _demandedTraff);
            Assert.AreEqual(0, _target.PeriodDetailAverage);
        }

        [Test]
        public void VerifySplitWhenOdd()
        {
            DateTimePeriod period = new DateTimePeriod(_start.AddMinutes(60),_start.AddMinutes(80));
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, period, 3, _demandedTraff);

			var resourceContainer = MockRepository.GenerateMock<IResourceCalculationDataContainerWithSingleOperation>();
			resourceContainer.Stub(x => x.IntraIntervalResources(_skillStaffPeriod.SkillDay.Skill, period)).Return(new[] { period });

			_target.ProcessLayers(resourceContainer, _skillStaffPeriod.SkillDay.Skill);

            Assert.AreEqual(3, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(2, _target.GetSplitPeriodValues()[6]);
            Assert.AreEqual(20, _target.PeriodDetailsSum);
        }

        [Test]
        public void VerifyCalculateNewDeviation()
        {
            DateTimePeriod period = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(75));
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, period, 5, _demandedTraff);

            var resourceContainer = MockRepository.GenerateMock<IResourceCalculationDataContainerWithSingleOperation>();
	        resourceContainer.Stub(x => x.IntraIntervalResources(_skillStaffPeriod.SkillDay.Skill, period))
		        .Return(new[] {new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(67))});

			_target.ProcessLayers(resourceContainer, _skillStaffPeriod.SkillDay.Skill);

            Assert.AreEqual(5, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(2, _target.GetSplitPeriodValues()[1]);
            Assert.AreEqual(7, _target.PeriodDetailsSum);

            double expected = Domain.Calculation.Variances.StandardDeviation(new[]{1d, -0.19d, -1d});
            Assert.AreEqual(expected, _target.CalculateStandardDeviation(), 0.01);
            Assert.AreEqual(2.33, _target.PeriodDetailAverage, 0.01);

			IList<IVisualLayer> lst = new List<IVisualLayer> { layerFactory.CreateShiftSetupLayer(_activity, period) };
            IVisualLayerCollection proj = new VisualLayerCollection(lst, new ProjectionPayloadMerger());
            
			// create one on the other 8 minutes
			var resourceContainer2 = MockRepository.GenerateMock<IResourceCalculationDataContainerWithSingleOperation>();
			resourceContainer2.Stub(x => x.IntraIntervalResources(_skillStaffPeriod.SkillDay.Skill, period))
				.Return(new[] { new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(75)) });

			_target.ProcessLayers(resourceContainer2, _skillStaffPeriod.SkillDay.Skill);

            Assert.AreEqual(0, _target.DeviationAfterNewLayers(proj));
        }

        [Test]
        public  void VerifyLayerFilteredOnLength()
        {
            DateTimePeriod period = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(75));
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, period, 5, _demandedTraff);

            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(90));
            
			var resourceContainer = MockRepository.GenerateMock<IResourceCalculationDataContainerWithSingleOperation>();
			resourceContainer.Stub(x => x.IntraIntervalResources(_skillStaffPeriod.SkillDay.Skill, period)).Return(new[] { period2 });

            _target.ProcessLayers(resourceContainer, _skillStaffPeriod.SkillDay.Skill);
            Assert.AreEqual(5, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(5, _target.GetSplitPeriodValues()[1]);
            Assert.AreEqual(15, _target.PeriodDetailsSum);
        }

        [Test]
        public void VerifyDistributionWhenSplit()
        {
            DateTimePeriod period = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(75));
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, period, 5, _demandedTraff);

            DateTimePeriod period1 = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(65));
            DateTimePeriod period3 = new DateTimePeriod(_start.AddMinutes(70), _start.AddMinutes(75));
			
			var resourceContainer = MockRepository.GenerateMock<IResourceCalculationDataContainerWithSingleOperation>();
			resourceContainer.Stub(x => x.IntraIntervalResources(_skillStaffPeriod.SkillDay.Skill, period)).Return(new[] { period1, period3 });

            _target.ProcessLayers(resourceContainer, _skillStaffPeriod.SkillDay.Skill);
            Assert.AreEqual(5, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(0, _target.GetSplitPeriodValues()[1]);
            Assert.AreEqual(5, _target.GetSplitPeriodValues()[2]);
            Assert.AreEqual(10, _target.PeriodDetailsSum);
        }

		[Test]
		public void VerifyMergeWhenSeveralResourcesOnSameInterval()
		{
			DateTimePeriod period = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(75));
			_target = new PeriodDistribution(_skillStaffPeriod, _activity, period, 5, _demandedTraff);

			DateTimePeriod period1 = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(65));
			DateTimePeriod period3 = new DateTimePeriod(_start.AddMinutes(70), _start.AddMinutes(75));

			var resourceContainer = MockRepository.GenerateMock<IResourceCalculationDataContainerWithSingleOperation>();
			resourceContainer.Stub(x => x.IntraIntervalResources(_skillStaffPeriod.SkillDay.Skill, period)).Return(new[] { period1, period1,period3 });

			_target.ProcessLayers(resourceContainer, _skillStaffPeriod.SkillDay.Skill);
			Assert.AreEqual(10, _target.GetSplitPeriodValues()[0]);
			Assert.AreEqual(0, _target.GetSplitPeriodValues()[1]);
			Assert.AreEqual(5, _target.GetSplitPeriodValues()[2]);
			Assert.AreEqual(15, _target.PeriodDetailsSum);
		}

        [Test]
        public void VerifyCorrectNumberOfDimensionsInResult()
        {
            Assert.AreEqual(3, _target.CalculateSplitPeriodRelativeValues().Length);
            _period = _period.ChangeEndTime(TimeSpan.FromMinutes(1));
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, _period, 5, _demandedTraff);
            Assert.AreEqual(4, _target.CalculateSplitPeriodRelativeValues().Length);
        }

        [Test]
        public void VerifyRelativeValuesWhenSplit()
        {
            DateTimePeriod period = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(75));
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, period, 5, _demandedTraff);

            DateTimePeriod period1 = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(65));
            DateTimePeriod period3 = new DateTimePeriod(_start.AddMinutes(70), _start.AddMinutes(75));

			var resourceContainer = MockRepository.GenerateMock<IResourceCalculationDataContainerWithSingleOperation>();
			resourceContainer.Stub(x => x.IntraIntervalResources(_skillStaffPeriod.SkillDay.Skill, period)).Return(new[] { period1,period3 });

            _target.ProcessLayers(resourceContainer, _skillStaffPeriod.SkillDay.Skill);
            DeviationStatisticData stat = new DeviationStatisticData(0.5,1);
            Assert.AreEqual(stat.RelativeDeviation, _target.CalculateSplitPeriodRelativeValues()[0], 0.001);
            
			stat = new DeviationStatisticData(0.5, 0);
            Assert.AreEqual(stat.RelativeDeviation, _target.CalculateSplitPeriodRelativeValues()[1]);
        }
    }
}
