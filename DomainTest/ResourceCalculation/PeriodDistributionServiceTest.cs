using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class PeriodDistributionServiceTest
    {
        private PeriodDistributionService _periodDistributionService;
        private IActivity _activity;
        private DateTime _start;
	    private MockRepository _mocks;
	    private IResourceCalculationDataContainer _container;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _activity = ActivityFactory.CreateActivity("adkfj");
			_activity.SetId(Guid.NewGuid());
		    _container = _mocks.StrictMock<IResourceCalculationDataContainer>();
            _periodDistributionService = new PeriodDistributionService();
            _start = new DateTime(2009, 2, 10, 8, 0, 0, DateTimeKind.Utc);
        }

        [Test]
        public void VerifySetup()
        {
            Assert.IsNotNull(_periodDistributionService);    
        }

        [Test]
        public void VerifyCalculateDay()
        {
            var period = new DateTimePeriod(_start, _start.AddMinutes(15));
            var task = new Task(5, new TimeSpan(0, 2, 0), new TimeSpan(0, 6, 0));
            var serviceAgreement = new ServiceAgreement(new ServiceLevel(new Percent(.8),20 ),new Percent(.9),new Percent(.7)  );

			var skillStaffPeriod = new SkillStaffPeriod(period, task, serviceAgreement);

            var skill = _mocks.StrictMock<ISkill>();
            var dicSkillStaffPeriods = new SkillStaffPeriodDictionary(skill);
            dicSkillStaffPeriods.Add(period, skillStaffPeriod);
            var dictionary = new SkillSkillStaffPeriodExtendedDictionary();
            dictionary.Add(skill, dicSkillStaffPeriods);

            _mocks.Record();
            Expect.Call(skill.Activity).Return(_activity).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

            _periodDistributionService.CalculateDay(_container, new SkillResourceCalculationPeriodWrapper(dictionary));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCalculateWithLayers()
        {
            _periodDistributionService = new PeriodDistributionService();

            var task = new Task(5, new TimeSpan(0, 2, 0), new TimeSpan(0, 6, 0));
            var serviceAgreement = new ServiceAgreement(new ServiceLevel(new Percent(.8), 20),
                                                                     new Percent(.9), new Percent(.7));
            var period = new DateTimePeriod(_start.AddHours(2), _start.AddHours(2).AddMinutes(15));
            var skillStaffPeriod = new SkillStaffPeriod(period, task, serviceAgreement);

            var mocks = new MockRepository();

            var skill = mocks.StrictMock<ISkill>();

            var dicSkillStaffPeriods = new SkillStaffPeriodDictionary(skill);
            dicSkillStaffPeriods.Add(period, skillStaffPeriod);
            var dictionary = new SkillSkillStaffPeriodExtendedDictionary();
            dictionary.Add(skill, dicSkillStaffPeriods);

            mocks.Record();
            Expect.Call(skill.Activity).Return(_activity).Repeat.AtLeastOnce();
            
            mocks.ReplayAll();

            _periodDistributionService.CalculateDay(_container, new SkillResourceCalculationPeriodWrapper(dictionary));
            double expedtedResult = Domain.Calculation.Variances.StandardDeviation(new[] { 0d, double.NaN, double.NaN });
            Assert.AreEqual(expedtedResult, skillStaffPeriod.IntraIntervalDeviation, 0.01);
        }
    }
}
