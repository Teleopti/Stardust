using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.DomainTest.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class DistributionPercentageSkillTest : RestrictionTest<ISkill>
    {
        IMultisiteSkill _testSkill;

        protected override void ConcreteSetup()
        {
            Target = new DistributionPercentage();

            _testSkill = SkillFactory.CreateMultisiteSkill("test",SkillTypeFactory.CreateSkillType(),15);
            _testSkill.TimeZone = (TimeZoneInfo.Utc);
        }

        protected override ISkill CreateInvalidEntityToVerify()
        {
			DateTime utcDateTime = _testSkill.TimeZone.SafeConvertTimeToUtc(SkillDayTemplate.BaseDate.Date);
            IChildSkill child = SkillFactory.CreateChildSkill("test1", _testSkill);
            Percent percentage = new Percent(0.4);
            TemplateMultisitePeriod invalidPeriod = new TemplateMultisitePeriod(
                new DateTimePeriod(utcDateTime, utcDateTime.AddDays(1)),
                new Dictionary<IChildSkill, Percent>());
            invalidPeriod.SetPercentage(child, percentage);
            MultisiteDayTemplate template = new MultisiteDayTemplate("invalid", new List<ITemplateMultisitePeriod>() { invalidPeriod });
            _testSkill.SetTemplateAt((int)DayOfWeek.Monday, template);
            return _testSkill;
        }

        protected override ISkill CreateValidEntityToVerify()
        {
			DateTime utcDateTime = _testSkill.TimeZone.SafeConvertTimeToUtc(SkillDayTemplate.BaseDate.Date);
            IChildSkill child1 = SkillFactory.CreateChildSkill("test1", _testSkill);
            Percent percentage1 = new Percent(0.4);
            IChildSkill child2 = SkillFactory.CreateChildSkill("test2", _testSkill);
            Percent percentage2 = new Percent(0.6);
            TemplateMultisitePeriod validPeriod = new TemplateMultisitePeriod(
                new DateTimePeriod(utcDateTime, utcDateTime.AddDays(1)),
                new Dictionary<IChildSkill, Percent>());
            validPeriod.SetPercentage(child1, percentage1);
            validPeriod.SetPercentage(child2, percentage2);
            MultisiteDayTemplate template = new MultisiteDayTemplate("invalid", new List<ITemplateMultisitePeriod>() { validPeriod });
            _testSkill.SetTemplateAt((int)DayOfWeek.Monday, template);
            return _testSkill;
        }

        /// <summary>
        /// Verifies the no exception when calling with ordinary skill.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        [Test]
        public void VerifyNoExceptionWhenCallingWithOrdinarySkill()
        {
            ISkill newSkill = SkillFactory.CreateSkill("test1");
            Target.CheckEntity(newSkill);
            Assert.IsNotNull(newSkill);
        }
    }

    [TestFixture]
    public class DistributionPercentageMultisiteDayTest : RestrictionTest<IMultisiteDay>
    {
        private MockRepository mocks;

        protected override void ConcreteSetup()
        {
            Target = new DistributionPercentage();
            mocks = new MockRepository();
        }

        protected override IMultisiteDay CreateInvalidEntityToVerify()
        {
            IMultisiteDay multisiteDay = mocks.StrictMock<IMultisiteDay>();
            IMultisitePeriod multisitePeriod = mocks.StrictMock<IMultisitePeriod>();

            Expect.Call(multisiteDay.MultisitePeriodCollection).Return(
                new ReadOnlyCollection<IMultisitePeriod>(new List<IMultisitePeriod> { multisitePeriod }));
            Expect.Call(multisitePeriod.IsValid).Return(false);

            mocks.ReplayAll();
            return multisiteDay;
        }

        protected override IMultisiteDay CreateValidEntityToVerify()
        {
            IMultisiteDay multisiteDay = mocks.StrictMock<IMultisiteDay>();
            IMultisitePeriod multisitePeriod = mocks.StrictMock<IMultisitePeriod>();

            Expect.Call(multisiteDay.MultisitePeriodCollection).Return(
                new ReadOnlyCollection<IMultisitePeriod>(new List<IMultisitePeriod> { multisitePeriod }));
            Expect.Call(multisitePeriod.IsValid).Return(true);

            mocks.ReplayAll();
            return multisiteDay;
        }

        [TearDown]
        public void Teardown()
        {
            mocks.VerifyAll();
        }
    }
}
