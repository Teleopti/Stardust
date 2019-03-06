using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Unit
{
	[TestFixture]
	public class RtaMapTest
	{
		[Test]
		public void VerifyHasEmptyConstructor()
		{
			var activity = new Activity();
			activity.SetId(Guid.NewGuid());
			var rtaStateGroup = new RtaStateGroup(" ");
			var target = new RtaMap
			{
				StateGroup = rtaStateGroup,
				Activity = activity.Id.Value
			};
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType(), true));
		}

		[Test]
		public void VerifyProperties()
		{
			var activity = new Activity();
			activity.SetId(Guid.NewGuid());
			var rtaStateGroup = new RtaStateGroup(" ");
			var rtaRule = new RtaRule();
			var target = new RtaMap
			{
				StateGroup = rtaStateGroup,
				Activity = activity.Id.Value
			};
			Assert.AreEqual(activity.Id.Value, target.Activity);
			Assert.AreEqual(rtaStateGroup, target.StateGroup);

			target.RtaRule = rtaRule;

			Assert.AreEqual(rtaRule, target.RtaRule);
		}

		[Test]
		public void ShouldClone()
		{
			var activity = new Activity();
			activity.SetId(Guid.NewGuid());
			var rtaStateGroup = new RtaStateGroup(" ");
			var target = new RtaMap
			{
				StateGroup = rtaStateGroup,
				Activity = activity.Id.Value
			};
			target.SetId(Guid.NewGuid());

			var clone = target.EntityClone();
			clone.Activity.Should().Be(activity.Id.Value);
			clone.Id.Should().Be(target.Id);
		}
	}
}