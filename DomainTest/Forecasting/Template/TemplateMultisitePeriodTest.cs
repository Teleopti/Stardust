using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Template
{
    /// <summary>
    /// Tests for TemplateMultisitePeriod
    /// </summary>
    [TestFixture]
    public class TemplateMultisitePeriodTest
    {
        private ITemplateMultisitePeriod target;
        private DateTimePeriod _dtp;
        private IDictionary<IChildSkill, Percent> _distribution;
        private IChildSkill _childSkill1;
        private IChildSkill _childSkill2;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _dtp = new DateTimePeriod(
                DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromHours(0)), DateTimeKind.Utc), 
                DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromDays(1)), DateTimeKind.Utc));
			
	        var skill = new MultisiteSkill("M", "", Color.Red, 15, SkillTypeFactory.CreateSkillType());
			_childSkill1 = new ChildSkill("Child1", "Child1", Color.Red, skill);
            _childSkill2 = new ChildSkill("Child2", "Child2", Color.Red, skill);

            _distribution = new Dictionary<IChildSkill, Percent>();
            _distribution[_childSkill1] = new Percent(0.6);
            _distribution[_childSkill2] = new Percent(0.4);
            target = new TemplateMultisitePeriod(_dtp,_distribution);
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
            Assert.IsNotNull(target);
            Assert.AreEqual(_dtp, target.Period);
            Assert.AreEqual(_distribution.Count, target.Distribution.Count);
            Assert.AreEqual(_distribution.Keys.First(), target.Distribution.Keys.First());
            Assert.IsNull(target.Version);
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
        /// Verifies the cannot set percentage with invalid child skill.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        [Test]
        public void VerifyCannotSetPercentageWithInvalidChildSkill()
        {
            IChildSkill temporaryChildSkill = new ChildSkill("3", "3", Color.Red, new MultisiteSkill("M", "", Color.Red, 15, SkillTypeFactory.CreateSkillType()));
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

            Assert.AreEqual(new Percent(0.65), target.Distribution[_childSkill1]);
            Assert.AreEqual(new Percent(0.35), target.Distribution[_childSkill2]);
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
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }

        [Test]
        public void VerifySetVersionThrows()
        {
            Assert.Throws<NotSupportedException>(() => ((TemplateMultisitePeriod) target).SetVersion(3));
        }
    }
}
