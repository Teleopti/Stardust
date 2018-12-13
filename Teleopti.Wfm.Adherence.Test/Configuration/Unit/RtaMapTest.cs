using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Unit
{
	[TestFixture]
	public class RtaMapTest
	{
		private RtaMap target;
		private IActivity activity;
		private IRtaStateGroup rtaStateGroup;
		private IRtaRule _rtaRule;

		[SetUp]
		public void Setup()
		{
			activity = new Activity();
			activity.SetId(Guid.NewGuid());
			rtaStateGroup = new RtaStateGroup(" ");
			_rtaRule = new RtaRule();
			target = new RtaMap
			{
				StateGroup = rtaStateGroup,
				Activity = activity.Id.Value
			};
		}

		[Test]
		public void VerifyHasEmptyConstructor()
		{
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType(), true));
		}

		[Test]
		public void VerifyProperties()
		{
			Assert.AreEqual(activity.Id.Value, target.Activity);
			Assert.AreEqual(rtaStateGroup, target.StateGroup);

			target.RtaRule = _rtaRule;

			Assert.AreEqual(_rtaRule, target.RtaRule);
		}

		[Test]
		public void ShouldClone()
		{
			target.SetId(Guid.NewGuid());

			var clone = target.EntityClone();
			clone.Activity.Should().Be(activity.Id.Value);
			clone.Id.Should().Be(target.Id);

			clone = target.NoneEntityClone();
			clone.Id.HasValue.Should().Be(false);
			clone.StateGroup.Should().Be(target.StateGroup);

			clone = (IRtaMap) target.Clone();
			clone.Id.Should().Be(target.Id);
			clone.RtaRule.Should().Be(target.RtaRule);
		}
	}
}