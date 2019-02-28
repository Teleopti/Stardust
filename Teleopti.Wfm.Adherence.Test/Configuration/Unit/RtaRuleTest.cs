using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Unit
{
	[TestFixture]
	public class RtaRuleTest
	{
		private readonly Description _description = new Description("Wrong state");
		private readonly Color _color = Color.DeepPink;
		private readonly int _thresholdTime = TimeSpan.FromSeconds(150).Seconds;
		private const double _staffingEffect = 1.0;

		[Test]
		public void VerifyProperties()
		{
			var target = new RtaRule(_description, _color, _thresholdTime, _staffingEffect);
			Assert.AreEqual(_description, target.Description);
			Assert.AreEqual(_color, target.DisplayColor);
			Assert.AreEqual(_thresholdTime, target.ThresholdTime);
			Assert.AreEqual(_staffingEffect, target.StaffingEffect);

			Description description = new Description("My new description");
			Color color = Color.Firebrick;
			int thresholdTime = TimeSpan.FromSeconds(73).Seconds;

			target.Description = description;
			target.DisplayColor = color;
			target.ThresholdTime = thresholdTime;
			target.StaffingEffect = 0.8;

			Assert.AreEqual(description, target.Description);
			Assert.AreEqual(color, target.DisplayColor);
			Assert.AreEqual(thresholdTime, target.ThresholdTime);
			Assert.AreEqual(0.8, target.StaffingEffect);
		}

		[Test]
		public void VerifyHasEmptyConstructor()
		{
			var target = new RtaRule(_description, _color, _thresholdTime, _staffingEffect);
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType(), true));
		}

		[Test]
		public void VerifyCannotHaveNegativeThresholdTime()
		{
			var target = new RtaRule(_description, _color, _thresholdTime, _staffingEffect);
			Assert.Throws<ArgumentOutOfRangeException>(() => target.ThresholdTime = TimeSpan.FromSeconds(-20).Seconds);
		}
	}
}
