using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class PeriodDistributionServiceTest
    {
        PeriodDistributionService _periodDistributionService;
        private IActivity _activity;
        private DateTime _start;
        private DateTime _end;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _activity = ActivityFactory.CreateActivity("adkfj");
            _periodDistributionService = new PeriodDistributionService(new List<IVisualLayerCollection>(), 5);
            _start = new DateTime(2009, 2, 10, 8, 0, 0, DateTimeKind.Utc);
            _end = _start.AddHours(9);
            _person = new Person();
        }

        [Test]
        public void VerifySetup()
        {
            Assert.IsNotNull(_periodDistributionService);    
        }

        [Test]
        public void VerifyCalculateDay()
        {
            DateTimePeriod period = new DateTimePeriod(_start, _start.AddMinutes(15));
            ITask task = new Task(5, new TimeSpan(0, 2, 0), new TimeSpan(0, 6, 0));
            ServiceAgreement serviceAgreement = new ServiceAgreement(new ServiceLevel(new Percent(.8),20 ),new Percent(.9),new Percent(.7)  );
            
            ISkillStaffPeriod skillStaffPeriod = new SkillStaffPeriod(period,task,serviceAgreement,new StaffingCalculatorService());

            MockRepository mocks = new MockRepository();

            ISkill skill = mocks.StrictMock<ISkill>();
            ISkillStaffPeriodDictionary dicSkillStaffPeriods = new SkillStaffPeriodDictionary(skill);
            dicSkillStaffPeriods.Add(period, skillStaffPeriod);
            ISkillSkillStaffPeriodExtendedDictionary dictionary = new SkillSkillStaffPeriodExtendedDictionary();
            dictionary.Add(skill, dicSkillStaffPeriods);

            mocks.Record();
            Expect.Call(skill.Activity).Return(_activity).Repeat.AtLeastOnce();

            mocks.ReplayAll();

            _periodDistributionService.CalculateDay(dictionary);
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCalculateWithLayers()
        {
            IVisualLayerFactory layerFactory = new VisualLayerFactory();
            DateTimePeriod period = new DateTimePeriod(_start, _end);
            VisualLayerProjectionService projectionService = new VisualLayerProjectionService(_person);
            IVisualLayer layer = layerFactory.CreateShiftSetupLayer(_activity, period);
            projectionService.Add(layer);

            DateTimePeriod breakPeriod = new DateTimePeriod(_start.AddHours(2), _start.AddHours(2).AddMinutes(7));
            IVisualLayer layer2 = layerFactory.CreateShiftSetupLayer(ActivityFactory.CreateActivity("l"), breakPeriod);
            projectionService.Add(layer2);

            var lst = new List<IVisualLayerCollection> { projectionService.CreateProjection() };

            _periodDistributionService = new PeriodDistributionService(lst, 5);

            ITask task = new Task(5, new TimeSpan(0, 2, 0), new TimeSpan(0, 6, 0));
            ServiceAgreement serviceAgreement = new ServiceAgreement(new ServiceLevel(new Percent(.8), 20),
                                                                     new Percent(.9), new Percent(.7));
            period = new DateTimePeriod(_start.AddHours(2), _start.AddHours(2).AddMinutes(15));
            ISkillStaffPeriod skillStaffPeriod = new SkillStaffPeriod(period, task, serviceAgreement,
                                                                      new StaffingCalculatorService());

            MockRepository mocks = new MockRepository();

            ISkill skill = mocks.StrictMock<ISkill>();

            ISkillStaffPeriodDictionary dicSkillStaffPeriods = new SkillStaffPeriodDictionary(skill);
            dicSkillStaffPeriods.Add(period, skillStaffPeriod);
            ISkillSkillStaffPeriodExtendedDictionary dictionary = new SkillSkillStaffPeriodExtendedDictionary();
            dictionary.Add(skill, dicSkillStaffPeriods);


            mocks.Record();
            Expect.Call(skill.Activity).Return(_activity).Repeat.AtLeastOnce();

            mocks.ReplayAll();

            _periodDistributionService.CalculateDay(dictionary);
            double expedtedResult = new PopulationStatisticsCalculator(new double[] { 0d, double.NaN, double.NaN }).StandardDeviation;
            Assert.AreEqual(expedtedResult, skillStaffPeriod.IntraIntervalDeviation, 0.01);
        }
    }
}
