using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class VisualLayerTest
	{
		private VisualLayer target;
		private DateTimePeriod period;
		private IActivity activity;
		private IVisualLayerFactory layerFactory;

		[SetUp]
		public void Setup()
		{
			layerFactory = new VisualLayerFactory();
			period = new DateTimePeriod(2000, 1, 1, 2001, 1, 1);
			activity = ActivityFactory.CreateActivity("df");
			target = (VisualLayer)layerFactory.CreateShiftSetupLayer(activity, period);
		}

		[Test]
		public void CanCreate()
		{
			Assert.IsNotNull(target);
			Assert.AreSame(activity, target.Payload);
			Assert.AreSame(activity, target.HighestPriorityActivity);
			Assert.AreEqual(period, target.Period);
			Assert.IsNull(target.HighestPriorityAbsence);
		}

		[Test]
		public void VerifyUnderlyingActivityMustNotBeNull()
		{
			Assert.Throws<ArgumentNullException>(() => layerFactory.CreateShiftSetupLayer(null, period));
		}

		[Test]
		public void ShouldCloneWithNewPeriod()
		{
			var newPeriod = new DateTimePeriod(2000, 5, 6, 2000, 7, 8);
			var res = (VisualLayer)target.CloneWithNewPeriod(newPeriod);
			res.Period.Should().Be.EqualTo(newPeriod);
			res.Payload.Should().Be.SameInstanceAs(target.Payload);
			res.HighestPriorityAbsence.Should().Be.SameInstanceAs(target.HighestPriorityAbsence);
			res.HighestPriorityActivity.Should().Be.SameInstanceAs(target.HighestPriorityActivity);
			res.DefinitionSet.Should().Be.SameInstanceAs(target.DefinitionSet);
		}
	}
}
