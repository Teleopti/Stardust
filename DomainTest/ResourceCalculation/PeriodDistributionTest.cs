using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
            IList<IVisualLayer> lst = new List<IVisualLayer>();
            lst.Add(layerFactory.CreateShiftSetupLayer(_activity, period, PersonFactory.CreatePerson()));
            IVisualLayerCollection proj = new VisualLayerCollection(null, lst, new ProjectionPayloadMerger());

            // create one just 15 minutes long
            proj = proj.FilterLayers(period);
            _target.ProcessLayers(proj);
            Assert.AreEqual(5, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(15,_target.PeriodDetailsSum);

            _target.ProcessLayers(proj);
            Assert.AreEqual(10, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(30, _target.PeriodDetailsSum);
           
        }

        [Test]
        public void VerifySplitWhenLayersShorter()
        {
            DateTimePeriod period = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(75));
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, period, 5, _demandedTraff);

            IList<IVisualLayer> lst = new List<IVisualLayer> { layerFactory.CreateShiftSetupLayer(_activity, period, PersonFactory.CreatePerson()) };
            IVisualLayerCollection proj = new VisualLayerCollection(null, lst, new ProjectionPayloadMerger());

            // create one just 7 minutes long
            proj = proj.FilterLayers(new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(67)));
            _target.ProcessLayers(proj);
            Assert.AreEqual(5, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(2, _target.GetSplitPeriodValues()[1]);
            Assert.AreEqual(0, _target.GetSplitPeriodValues()[2]);
            Assert.AreEqual(7, _target.PeriodDetailsSum);

            double expected = 
                new PopulationStatisticsCalculator(new double[] {1d, -0.19d, -1d}).StandardDeviation;
            Assert.AreEqual(expected, _target.CalculateStandardDeviation(), 0.01);

            Assert.AreEqual(2.33, _target.PeriodDetailAverage, 0.01);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyWhenZero()
        {
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, _period, 0, _demandedTraff);
            Assert.AreEqual(0,_target.PeriodDetailAverage);
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
            IList<IVisualLayer> lst = new List<IVisualLayer>();
            lst.Add(layerFactory.CreateShiftSetupLayer(_activity, period, PersonFactory.CreatePerson()));
            IVisualLayerCollection proj = new VisualLayerCollection(null, lst, new ProjectionPayloadMerger());

            // create one just 20 minutes long
            proj = proj.FilterLayers(new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(80)));
            _target.ProcessLayers(proj);
            Assert.AreEqual(3, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(2, _target.GetSplitPeriodValues()[6]);
            Assert.AreEqual(20, _target.PeriodDetailsSum);
        }

        [Test]
        public void VerifyCalculateNewDeviation()
        {
            DateTimePeriod period = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(75));
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, period, 5, _demandedTraff);

            IList<IVisualLayer> lst = new List<IVisualLayer> { layerFactory.CreateShiftSetupLayer(_activity, period, PersonFactory.CreatePerson()) };
            IVisualLayerCollection proj = new VisualLayerCollection(null, lst, new ProjectionPayloadMerger());

            // create one just 7 minutes long
            proj = proj.FilterLayers(new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(67)));
            _target.ProcessLayers(proj);
            Assert.AreEqual(5, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(2, _target.GetSplitPeriodValues()[1]);
            Assert.AreEqual(7, _target.PeriodDetailsSum);

            double expected = new PopulationStatisticsCalculator(new double[]{1d, -0.19d, -1d}).StandardDeviation;
            Assert.AreEqual(expected, _target.CalculateStandardDeviation(), 0.01);
            Assert.AreEqual(2.33, _target.PeriodDetailAverage, 0.01);

            proj = new VisualLayerCollection(null, lst, new ProjectionPayloadMerger());
            // create one on the other 8 minutes
            proj = proj.FilterLayers(new DateTimePeriod(_start.AddMinutes(67), _start.AddMinutes(75)));

            Assert.AreEqual(0, _target.DeviationAfterNewLayers(proj));
        }

        [Test]
        public  void VerifyLayerFilteredOnLength()
        {
            DateTimePeriod period = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(75));
            _target = new PeriodDistribution(_skillStaffPeriod, _activity, period, 5, _demandedTraff);

            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(60), _start.AddMinutes(90));
            IList<IVisualLayer> lst = new List<IVisualLayer> { layerFactory.CreateShiftSetupLayer(_activity, period2, PersonFactory.CreatePerson()) };
            IVisualLayerCollection proj = new VisualLayerCollection(null, lst, new ProjectionPayloadMerger());
            _target.ProcessLayers(proj);
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
            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(65), _start.AddMinutes(70));
            DateTimePeriod period3 = new DateTimePeriod(_start.AddMinutes(70), _start.AddMinutes(75));

            IActivity activity2 = ActivityFactory.CreateActivity("Break");
        	IPerson person = PersonFactory.CreatePerson();
            IVisualLayer layer1 =  layerFactory.CreateShiftSetupLayer(_activity, period1, person);
            IVisualLayer layer2 = layerFactory.CreateShiftSetupLayer(activity2, period2, person);
            IVisualLayer layer3 = layerFactory.CreateShiftSetupLayer(_activity, period3, person);
            IList<IVisualLayer> lst = new List<IVisualLayer> { layer1,layer2,layer3 };
            IVisualLayerCollection proj = new VisualLayerCollection(null, lst, new ProjectionPayloadMerger());
            _target.ProcessLayers(proj);
            Assert.AreEqual(5, _target.GetSplitPeriodValues()[0]);
            Assert.AreEqual(0, _target.GetSplitPeriodValues()[1]);
            Assert.AreEqual(5, _target.GetSplitPeriodValues()[2]);
            Assert.AreEqual(10, _target.PeriodDetailsSum);
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
            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(65), _start.AddMinutes(70));
            DateTimePeriod period3 = new DateTimePeriod(_start.AddMinutes(70), _start.AddMinutes(75));

            IActivity activity2 = ActivityFactory.CreateActivity("Break");
        	IPerson person = PersonFactory.CreatePerson();
            IVisualLayer layer1 = layerFactory.CreateShiftSetupLayer(_activity, period1,person);
            IVisualLayer layer2 = layerFactory.CreateShiftSetupLayer(activity2, period2, person);
            IVisualLayer layer3 = layerFactory.CreateShiftSetupLayer(_activity, period3, person);
            IList<IVisualLayer> lst = new List<IVisualLayer> { layer1, layer2, layer3 };
            IVisualLayerCollection proj = new VisualLayerCollection(null, lst, new ProjectionPayloadMerger());
            _target.ProcessLayers(proj);
            DeviationStatisticData stat = new DeviationStatisticData(0.5,1);
            Assert.AreEqual(stat.RelativeDeviation, _target.CalculateSplitPeriodRelativeValues()[0], 0.001);
            //Assert.AreEqual(1d, _target.CalculateSplitPeriodRelativeValues()[0], 0.001);
            stat = new DeviationStatisticData(0.5, 0);
            Assert.AreEqual(stat.RelativeDeviation, _target.CalculateSplitPeriodRelativeValues()[1]);
            //Assert.AreEqual(-1d, _target.CalculateSplitPeriodRelativeValues()[1]);
            //Assert.AreEqual(1d, _target.CalculateSplitPeriodRelativeValues()[2], 0.001);
        }
    }
}
