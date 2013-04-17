using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
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
        private PeriodDistributionService _periodDistributionService;
        private IActivity _activity;
        private DateTime _start;
        private DateTime _end;
        private IPerson _person;
	    private Guid _skillId;
	    private MockRepository _mocks;
	    private IResourceCalculationDataContainer _container;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _activity = ActivityFactory.CreateActivity("adkfj");
			_activity.SetId(Guid.NewGuid());
	        _skillId = Guid.NewGuid();
		    _container = _mocks.StrictMock<IResourceCalculationDataContainer>();
            _periodDistributionService = new PeriodDistributionService(_container, 5);
            _start = new DateTime(2009, 2, 10, 8, 0, 0, DateTimeKind.Utc);
            _end = _start.AddHours(9);
            _person = PersonFactory.CreatePerson();
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

            ISkill skill = _mocks.StrictMock<ISkill>();
            ISkillStaffPeriodDictionary dicSkillStaffPeriods = new SkillStaffPeriodDictionary(skill);
            dicSkillStaffPeriods.Add(period, skillStaffPeriod);
            ISkillSkillStaffPeriodExtendedDictionary dictionary = new SkillSkillStaffPeriodExtendedDictionary();
            dictionary.Add(skill, dicSkillStaffPeriods);

            _mocks.Record();
            Expect.Call(skill.Activity).Return(_activity).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

            _periodDistributionService.CalculateDay(dictionary);
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCalculateWithLayers()
        {
            _periodDistributionService = new PeriodDistributionService(_container, 5);

            ITask task = new Task(5, new TimeSpan(0, 2, 0), new TimeSpan(0, 6, 0));
            ServiceAgreement serviceAgreement = new ServiceAgreement(new ServiceLevel(new Percent(.8), 20),
                                                                     new Percent(.9), new Percent(.7));
            var period = new DateTimePeriod(_start.AddHours(2), _start.AddHours(2).AddMinutes(15));
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
