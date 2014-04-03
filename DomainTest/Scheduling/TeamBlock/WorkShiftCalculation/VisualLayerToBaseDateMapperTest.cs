using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	[TestFixture]
	public class VisualLayerToBaseDateMapperTest
	{
		private IVisualLayerToBaseDateMapper _target;

		[SetUp]
		public void Setup()
		{
			_target = new VisualLayerToBaseDateMapper();
		}

		[Test]
		public void ShouldDoTheBasicMappingWithoutAnyMidnightCrossings()
		{
			var start = new DateTime(2014, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc);
			var end = start.AddHours(1);
			var layerToMapPeriod = new DateTimePeriod(start.AddMinutes(15), end);
			IVisualLayer layerToMap = new VisualLayerFactory().CreateShiftSetupLayer(new Activity("activity"), layerToMapPeriod,
				new Person());

			var firstLayerInShiftPeriod = new DateTimePeriod(start, end);
			IVisualLayer firstLayerInShift = new VisualLayerFactory().CreateShiftSetupLayer(new Activity("activity"), firstLayerInShiftPeriod,
				new Person());

			TimeSpan result = _target.Map(layerToMap, firstLayerInShift, 30);
			Assert.AreEqual(TimeSpan.FromHours(12), result);
		}

		[Test]
		public void ShouldReturnCorrectIfLayerToMapStartsAfterMidnight()
		{
			var start = new DateTime(2014, 4, 1, 23, 45, 0, 0, DateTimeKind.Utc);
			var end = start.AddHours(1);

			var firstLayerInShiftPeriod = new DateTimePeriod(start, end);
			IVisualLayer firstLayerInShift = new VisualLayerFactory().CreateShiftSetupLayer(new Activity("activity"), firstLayerInShiftPeriod,
				new Person());

			var layerToMapPeriod = new DateTimePeriod(start.AddMinutes(60), end.AddMinutes(60));
			IVisualLayer layerToMap = new VisualLayerFactory().CreateShiftSetupLayer(new Activity("activity"), layerToMapPeriod,
				new Person());

			TimeSpan result = _target.Map(layerToMap, firstLayerInShift, 30);
			Assert.AreEqual(new TimeSpan(1, 0, 30, 0), result);
		}

	}
}