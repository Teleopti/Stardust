using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
	[TestFixture]
	public class RuleTest
	{
		private RtaRule _target;
		private readonly Description _description = new Description("Wrong state");
		private readonly Color _color = Color.DeepPink;
		private readonly TimeSpan _thresholdTime = TimeSpan.FromSeconds(150);
		private const double _staffingEffect = 1.0;

		[SetUp]
		public void Setup()
		{
			_target = new RtaRule(_description, _color, _thresholdTime, _staffingEffect);
		}

		[Test]
		public void VerifyProperties()
		{
			Assert.AreEqual(_description, _target.Description);
			Assert.AreEqual(_description, _target.ConfidentialDescription(null));
			Assert.AreEqual(_color, _target.DisplayColor);
			Assert.AreEqual(_color, _target.ConfidentialDisplayColor(null));
			Assert.AreEqual(_thresholdTime, _target.ThresholdTime);
			Assert.AreEqual(_staffingEffect, _target.StaffingEffect);
			Assert.IsNull(_target.Tracker);
			Assert.IsFalse(_target.InContractTime);

			Description description = new Description("My new description");
			Color color = Color.Firebrick;
			TimeSpan thresholdTime = TimeSpan.FromSeconds(73);

			_target.Description = description;
			_target.DisplayColor = color;
			_target.ThresholdTime = thresholdTime;
			_target.Tracker = null;
			_target.InContractTime = true;
			_target.StaffingEffect = 0.8;


			Assert.AreEqual(description, _target.Description);
			Assert.AreEqual(color, _target.DisplayColor);
			Assert.AreEqual(thresholdTime, _target.ThresholdTime);
			Assert.IsNull(_target.Tracker);
			Assert.IsTrue(_target.InContractTime);
			Assert.AreEqual(0.8, _target.StaffingEffect);
		}

		[Test]
		public void VerifyHasEmptyConstructor()
		{
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(), true));
		}

		[Test, ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void VerifyCannotHaveNegativeThresholdTime()
		{
			_target.ThresholdTime = TimeSpan.FromSeconds(-20);
		}


		[Test]
		public void VerifySetDeleted()
		{
			_target.SetDeleted();
			Assert.IsTrue(_target.IsDeleted);
		}

		[Test]
		public void ShouldNotThrowWhenNoAdherence()
		{
			var target = new RtaRule();

			Assert.DoesNotThrow(() =>
			{
				var action = target.AdherenceText;
			});
		}
	}
}
