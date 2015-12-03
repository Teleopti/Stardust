using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
    [TestFixture]
    public class StateGroupActivityAlarmTest
    {
        private StateGroupActivityAlarm target;
        private MockRepository mocks;
        private IActivity activity;
        private IRtaStateGroup rtaStateGroup;
        private IRtaRule _rtaRule;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            activity = mocks.StrictMock<IActivity>();
            rtaStateGroup = mocks.StrictMock<IRtaStateGroup>();
            _rtaRule = mocks.StrictMock<IRtaRule>();
            target = new StateGroupActivityAlarm(rtaStateGroup,activity);
        }

        [Test]
        public void VerifyHasEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(activity,target.Activity);
            Assert.AreEqual(rtaStateGroup,target.StateGroup);

            target.RtaRule = _rtaRule;

            Assert.AreEqual(_rtaRule,target.RtaRule);
        }

		[Test]
		public void ShouldClone()
		{
			target.SetId(Guid.NewGuid());

			var clone = target.EntityClone();
			clone.Activity.Should().Be(activity);
			clone.Id.Should().Be(target.Id);

			clone = target.NoneEntityClone();
			clone.Id.HasValue.Should().Be(false);
			clone.StateGroup.Should().Be(target.StateGroup);

			clone = (IStateGroupActivityAlarm)target.Clone();
			clone.Id.Should().Be(target.Id);
			clone.RtaRule.Should().Be(target.RtaRule);
		}
    }
}
