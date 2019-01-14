using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class MultisitePeriodTest
    {
        private IMultisitePeriod target;
        private DateTimePeriod _period;
        private IDictionary<IChildSkill, Percent> _distribution;
        private ChildSkill _childSkill1;
        private ChildSkill _childSkill2;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            _period = new DateTimePeriod(
                DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromHours(0)), DateTimeKind.Utc),
                DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromDays(1)), DateTimeKind.Utc));
            ISkillType skillType = SkillTypeFactory.CreateSkillTypePhone();
			var skill = new MultisiteSkill("M","",Color.Red,15,skillType);
            _childSkill1 = new ChildSkill("Child1", "Child1", Color.Red, skill);
            _childSkill2 = new ChildSkill("Child2", "Child2", Color.Red, skill);

            _distribution = new Dictionary<IChildSkill, Percent>();
            _distribution[_childSkill1] = new Percent(0.6);
            _distribution[_childSkill2] = new Percent(0.4);

            target = new MultisitePeriod(_period, _distribution);
        }

        /// <summary>
        /// Verifies the properties are set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        [Test]
        public void VerifyPropertiesAreSet()
        {
            Assert.IsNotNull(target);
            Assert.AreEqual(_period, target.Period);
            Assert.AreEqual(_distribution.Count, target.Distribution.Count);
            Assert.AreEqual(_distribution.Keys.First(), target.Distribution.Keys.First());
        }

        /// <summary>
        /// Verifies the can set percentage.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        [Test]
        public void VerifyCanSetPercentage()
        {
            target.SetPercentage(_childSkill1, new Percent(0.65));
            target.SetPercentage(_childSkill2, new Percent(0.35));

            Assert.AreEqual(new Percent(0.65),target.Distribution[_childSkill1]);
            Assert.AreEqual(new Percent(0.35), target.Distribution[_childSkill2]);
        }

        /// <summary>
        /// Verifies the cannot set percentage with invalid child skill.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        [Test]
        public void VerifyCanSetPercentageWithInvalidChildSkill()
        {
	        var temporaryChildSkill = new ChildSkill("3", "3", Color.Red,
		        new MultisiteSkill("M", "", Color.Red, 15, SkillTypeFactory.CreateSkillTypePhone()));
            target.SetPercentage(_childSkill1, new Percent(0.65));
            target.SetPercentage(
                temporaryChildSkill, 
                new Percent(0.35));

            Assert.AreEqual(new Percent(0.65), target.Distribution[_childSkill1]);
            Assert.AreEqual(new Percent(0.4), target.Distribution[_childSkill2]);
            Assert.AreEqual(new Percent(0.35), target.Distribution[temporaryChildSkill]);
            Assert.IsFalse(target.IsValid);
        }

        /// <summary>
        /// Verifies the is valid works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        [Test]
        public void VerifyIsValidWorks()
        {
            Assert.IsTrue(target.IsValid);
            target.SetPercentage(_childSkill1, new Percent(0.7));
            Assert.IsFalse(target.IsValid);
            target.SetPercentage(_childSkill2, new Percent(0.3));
            Assert.IsTrue(target.IsValid);
        }

        /// <summary>
        /// Verifies the protected constructor.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
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
            target.SetId(Guid.NewGuid());
            IMultisitePeriod multisitePeriodClone = (IMultisitePeriod)target.Clone();
            Assert.IsFalse(multisitePeriodClone.Id.HasValue);
            Assert.AreEqual(target.Distribution.Count, multisitePeriodClone.Distribution.Count);
            Assert.AreNotSame(target.Distribution, multisitePeriodClone.Distribution);
            Assert.AreEqual(target.Period, multisitePeriodClone.Period);

            multisitePeriodClone = target.NoneEntityClone();
            Assert.IsFalse(multisitePeriodClone.Id.HasValue);
            Assert.AreEqual(target.Distribution.Count, multisitePeriodClone.Distribution.Count);
            Assert.AreNotSame(target.Distribution, multisitePeriodClone.Distribution);
            Assert.AreEqual(target.Period, multisitePeriodClone.Period);

            multisitePeriodClone = target.EntityClone();
            Assert.AreEqual(target.Id.Value, multisitePeriodClone.Id.Value);
            Assert.AreEqual(target.Distribution.Count, multisitePeriodClone.Distribution.Count);
            Assert.AreNotSame(target.Distribution, multisitePeriodClone.Distribution);
            Assert.AreEqual(target.Period, multisitePeriodClone.Period);
        }
    }
}