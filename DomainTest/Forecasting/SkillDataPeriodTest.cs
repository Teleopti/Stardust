using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class SkillDataPeriodTest
    {
        private ISkillDataPeriod _target;
        private ServiceAgreement _serviceAgreement;
        private SkillPersonData _skillPersonData;
        private DateTimePeriod _period;
        private Percent _shrinkage;
        private Percent _efficiency;
    	private double? _manualAgents;

    	[SetUp]
        public void Setup()
        {
            _serviceAgreement = ServiceAgreement.DefaultValues();
            _skillPersonData = new SkillPersonData();

            _period = new DateTimePeriod(
                DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromHours(0)), DateTimeKind.Utc),
                DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromDays(1)), DateTimeKind.Utc));
            _target = new SkillDataPeriod(_serviceAgreement, _skillPersonData, _period);
            _shrinkage = new Percent(0.1);
            _efficiency = new Percent(1);
    		_manualAgents = 150d;
        }

        [Test]
        public void VerifyPropertiesAreSet()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(_serviceAgreement, _target.ServiceAgreement);
            Assert.AreEqual(_period, _target.Period);
            Assert.AreEqual(_skillPersonData, _target.SkillPersonData);
            Assert.AreEqual(new Percent(0), _target.Shrinkage);
            _target.Shrinkage = _shrinkage;
            Assert.AreEqual(_shrinkage, _target.Shrinkage);
            Assert.AreEqual(_efficiency, _target.Efficiency);
            _target.Efficiency = _efficiency;
            Assert.AreEqual(_efficiency, _target.Efficiency);
			Assert.IsNull(_target.ManualAgents);
			_target.ManualAgents = _manualAgents;
			Assert.AreEqual(150d, _target.ManualAgents);
        }

        /// <summary>
        /// Verifies the skill person data can be set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-30
        /// </remarks>
        [Test]
        public void VerifySkillPersonDataCanBeSet()
        {
            SkillPersonData skillPersonData = new SkillPersonData(10,20);
            _target.SkillPersonData = skillPersonData;

            Assert.AreEqual(skillPersonData, _target.SkillPersonData);
        }

        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        /// <summary>
        /// Verifies the clone works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-11
        /// </remarks>
        [Test]
        public void VerifyCloneWorks()
        {
            SkillDataPeriod clonedEntity = (SkillDataPeriod)_target.Clone();
            Assert.AreNotEqual(clonedEntity, _target);
            Assert.AreNotSame(clonedEntity, _target);
        }

        /// <summary>
        /// Verifies the service agreement can be set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-14
        /// </remarks>
        [Test]
        public void VerifyServiceAgreementCanBeSet()
        {
            ServiceAgreement newServiceAgreement = new ServiceAgreement(ServiceAgreement.DefaultValues().ServiceLevel,
                new Percent(0.4),new Percent(0.6));
            _target.ServiceAgreement = newServiceAgreement;

            Assert.AreEqual(newServiceAgreement, _target.ServiceAgreement);
        }

        /// <summary>
        /// Verifies the service agreement properties can be set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-08
        /// </remarks>
        [Test]
        public void VerifyServiceAgreementPropertiesCanBeSet()
        {
            _target.MinOccupancy = new Percent(0.05);
            _target.MaxOccupancy = new Percent(0.93);
            _target.ServiceLevelPercent = new Percent(0.7);
            _target.ServiceLevelSeconds = 23d;
            _target.MinimumPersons = 4;
            _target.MaximumPersons = 6;

            Assert.AreEqual(new Percent(0.05), _target.ServiceAgreement.MinOccupancy);
            Assert.AreEqual(new Percent(0.93), _target.ServiceAgreement.MaxOccupancy);
            Assert.AreEqual(new Percent(0.05), _target.MinOccupancy);
            Assert.AreEqual(new Percent(0.93), _target.MaxOccupancy);

            Assert.AreEqual(new Percent(0.7), _target.ServiceAgreement.ServiceLevel.Percent);
            Assert.AreEqual(23d, _target.ServiceAgreement.ServiceLevel.Seconds);
            Assert.AreEqual(4,_target.SkillPersonData.MinimumPersons);
            Assert.AreEqual(6,_target.SkillPersonData.MaximumPersons);

            Assert.AreEqual(new Percent(0.7), _target.ServiceLevelPercent);
            Assert.AreEqual(23d, _target.ServiceLevelSeconds);
            Assert.AreEqual(23d, _target.ServiceLevelTimeSpan.TotalSeconds);
            Assert.AreEqual(4, _target.MinimumPersons);
            Assert.AreEqual(6, _target.MaximumPersons);
            _target.ServiceLevelTimeSpan = new TimeSpan(0, 1, 20, 40);
            Assert.AreEqual(new TimeSpan(0, 1, 20, 40), _target.ServiceLevelTimeSpan);
            Assert.AreEqual(new TimeSpan(0, 1, 20, 40).TotalSeconds, _target.ServiceLevelSeconds);
        }

        /// <summary>
        /// Verifies the none entity clone.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-12-15
        /// </remarks>
        [Test]
        public void VerifyNoneEntityClone()
        {
            _target.SetId(Guid.NewGuid());
            var taskPeriodClone = (ISkillDataPeriod)_target.Clone();
            Assert.IsFalse(taskPeriodClone.Id.HasValue);
            Assert.AreEqual(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreNotSame(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreEqual(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
            Assert.AreNotSame(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
            Assert.AreEqual(_target.Shrinkage, taskPeriodClone.Shrinkage);
            Assert.AreEqual(_target.Efficiency, taskPeriodClone.Efficiency);
            Assert.AreEqual(_target.ManualAgents, taskPeriodClone.ManualAgents);
            Assert.AreEqual(_target.Period, taskPeriodClone.Period);

            taskPeriodClone = _target.NoneEntityClone();
            Assert.IsFalse(taskPeriodClone.Id.HasValue);
            Assert.AreEqual(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreNotSame(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreEqual(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
            Assert.AreNotSame(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
            Assert.AreEqual(_target.Shrinkage, taskPeriodClone.Shrinkage);
            Assert.AreEqual(_target.Efficiency, taskPeriodClone.Efficiency);
            Assert.AreEqual(_target.ManualAgents, taskPeriodClone.ManualAgents);
            Assert.AreEqual(_target.Period, taskPeriodClone.Period);

            taskPeriodClone = _target.EntityClone();
            Assert.AreEqual(_target.Id.Value, taskPeriodClone.Id.Value);
            Assert.AreEqual(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreNotSame(_target.ServiceAgreement, taskPeriodClone.ServiceAgreement);
            Assert.AreEqual(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
            Assert.AreNotSame(_target.SkillPersonData, taskPeriodClone.SkillPersonData);
            Assert.AreEqual(_target.Shrinkage, taskPeriodClone.Shrinkage);
            Assert.AreEqual(_target.Efficiency, taskPeriodClone.Efficiency);
            Assert.AreEqual(_target.ManualAgents, taskPeriodClone.ManualAgents);
            Assert.AreEqual(_target.Period, taskPeriodClone.Period);
        }
    }
}