using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Forecasting.Template
{
    /// <summary>
    /// Tests for TemplateSkillDataPeriod
    /// </summary>
    [TestFixture]
    public class TemplateSkillDataPeriodTest
    {
        private ITemplateSkillDataPeriod _target;
        private ServiceAgreement _serviceAgreement;
        private SkillPersonData _skillPersonData;
        private DateTimePeriod _dtp;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _serviceAgreement = ServiceAgreement.DefaultValues();
            _skillPersonData = new SkillPersonData();
            _dtp = new DateTimePeriod(
                DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromHours(0)), DateTimeKind.Utc), 
                DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromDays(1)), DateTimeKind.Utc));

            _target = new TemplateSkillDataPeriod(_serviceAgreement, _skillPersonData, _dtp);
        }


        /// <summary>
        /// Determines whether this instance [can set and get properties].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-11
        /// </remarks>
        [Test]
        public void CanSetAndGetProperties()
        {
            Assert.AreEqual(_serviceAgreement, _target.ServiceAgreement);
            Assert.AreEqual(_skillPersonData, _target.SkillPersonData);
            Assert.AreEqual(_dtp, _target.Period);

            var newSkillPersonData = new SkillPersonData(2, 10);
            _target.SkillPersonData = newSkillPersonData;

            Assert.AreEqual(newSkillPersonData, _target.SkillPersonData);

            var newMinOccupancy = new Percent(0.35);
            _target.MinOccupancy = newMinOccupancy;
            var newMaxOccupancy = new Percent(0.99);
            _target.MaxOccupancy = newMaxOccupancy;
            var newMaximumPersons = 10000;
            _target.MaximumPersons = newMaximumPersons;
            var newServiceLevelSeconds = 90;
            _target.ServiceLevelSeconds = newServiceLevelSeconds;

            Assert.AreEqual(newMinOccupancy, _target.MinOccupancy);
            Assert.AreEqual(newMaxOccupancy, _target.MaxOccupancy);
            Assert.AreEqual(newMaximumPersons, _target.MaximumPersons);
            Assert.AreEqual(newServiceLevelSeconds, _target.ServiceLevelSeconds);
        }

        /// <summary>
        /// Verifies the protected or private constructor.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-11
        /// </remarks>
        [Test]
        public void VerifyProtectedOrPrivateConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        [Test]
        public void CanClone()
        {
            _target.SetId(Guid.NewGuid());
            var taskPeriodClone = (ITemplateSkillDataPeriod)_target.Clone();
            Assert.IsFalse(taskPeriodClone.Id.HasValue);
            Assert.AreEqual(_target.Shrinkage, taskPeriodClone.Shrinkage);
            Assert.AreEqual(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreNotSame(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreEqual(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
            Assert.AreNotSame(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
			Assert.AreEqual(_target.Efficiency, taskPeriodClone.Efficiency);
            Assert.AreEqual(_target.ManualAgents, taskPeriodClone.ManualAgents);
            Assert.AreEqual(_target.Period, taskPeriodClone.Period);

            taskPeriodClone = _target.NoneEntityClone();
            Assert.IsFalse(taskPeriodClone.Id.HasValue);
            Assert.AreEqual(_target.Shrinkage, taskPeriodClone.Shrinkage);
            Assert.AreEqual(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreNotSame(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreEqual(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
            Assert.AreNotSame(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
			Assert.AreEqual(_target.Efficiency, taskPeriodClone.Efficiency);
            Assert.AreEqual(_target.ManualAgents, taskPeriodClone.ManualAgents);
            Assert.AreEqual(_target.Period, taskPeriodClone.Period);

            taskPeriodClone = _target.EntityClone();
            Assert.AreEqual(_target.Id.Value, taskPeriodClone.Id.Value);
            Assert.AreEqual(_target.Shrinkage, taskPeriodClone.Shrinkage);
            Assert.AreEqual(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreNotSame(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreEqual(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
            Assert.AreNotSame(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
			Assert.AreEqual(_target.Efficiency, taskPeriodClone.Efficiency);
            Assert.AreEqual(_target.ManualAgents, taskPeriodClone.ManualAgents);
            Assert.AreEqual(_target.Period, taskPeriodClone.Period);
        }
    }
}
