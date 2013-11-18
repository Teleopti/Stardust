using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
	[TestFixture]
	public class VisualLayerCollectionExtensionsTest
	{
		private readonly IPerson person = PersonFactory.CreatePerson();
		private readonly IActivity activity = ActivityFactory.CreateActivity("Phone");
		private readonly IActivity secondActivity = ActivityFactory.CreateActivity("Break");

		[Test]
		public void ShouldReturnFractionOfResourceWhenStartWithinIntervalWithTwoLayers()
		{
			var visualLayer1 = new VisualLayer(secondActivity, new DateTimePeriod(new DateTime(2001, 1, 1, 9, 45, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 10, 12, 0, DateTimeKind.Utc)),
											  secondActivity, person);
			var visualLayer2 = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 12, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 10, 15, 0, DateTimeKind.Utc)),
											  activity, person);
			var visualLayer3 = new VisualLayer(secondActivity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 15, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 10, 30, 0, DateTimeKind.Utc)),
											  secondActivity, person);

			var layers = new List<IVisualLayer> { visualLayer1, visualLayer2, visualLayer3 };
			var visualLayers = new VisualLayerCollection(person, layers, new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15).ToArray();

			result.First(r => r.Period.StartDateTime == new DateTime(2001, 1, 1, 10, 0, 0, DateTimeKind.Utc)).Resource.Should().Be.EqualTo(0.8);
			result.Last(r => r.Period.StartDateTime == new DateTime(2001, 1, 1, 10, 0, 0, DateTimeKind.Utc)).Resource.Should().Be.EqualTo(0.2);
		}
	
		[Test]
		public void ShouldReturnFractionOfResourceWhenStartWithinInterval()
		{
			var visualLayer = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 12, 0, DateTimeKind.Utc),
			                                                               new DateTime(2001, 1, 1, 10, 15, 0, DateTimeKind.Utc)),
			                                  activity, person);
			
			var layers = new List<IVisualLayer> { visualLayer };
			var visualLayers = new VisualLayerCollection(person,layers,new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15).ToArray();

			var resultLayer = result.Single();
			resultLayer.Resource.Should().Be.EqualTo(0.2);
			resultLayer.FractionPeriod.Value.Should().Be.EqualTo(visualLayer.Period);
		}

		[Test]
		public void ShouldReturnFractionOfResourceWhenEndWithinInterval()
		{
			var visualLayer = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 45, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 10, 48, 0, DateTimeKind.Utc)),
											  activity, person);

			var layers = new List<IVisualLayer> { visualLayer };
			var visualLayers = new VisualLayerCollection(person, layers, new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15).ToArray();

			var resultLayer = result.Single();
			resultLayer.Resource.Should().Be.EqualTo(0.2);
			resultLayer.FractionPeriod.Value.Should().Be.EqualTo(visualLayer.Period);
		}

		[Test]
		public void ShouldReturnFractionOfResourceWhenBothStartAndEndWithinInterval()
		{
			var visualLayer = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 48, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 10, 51, 0, DateTimeKind.Utc)),
											  activity, person);

			var layers = new List<IVisualLayer> { visualLayer };
			var visualLayers = new VisualLayerCollection(person, layers, new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15).ToArray();

			var resultLayer = result.Single();
			resultLayer.Resource.Should().Be.EqualTo(0.2);
			resultLayer.FractionPeriod.Value.Should().Be.EqualTo(visualLayer.Period);
		}

		[Test]
		public void ShouldReturnResourceWhenStartAndEndOnInterval()
		{
			var visualLayer = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 45, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc)),
											  activity, person);

			var layers = new List<IVisualLayer> { visualLayer };
			var visualLayers = new VisualLayerCollection(person, layers, new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15).ToArray();

			result.Single().Resource.Should().Be.EqualTo(1.0);
		}

		[Test]
		public void ShouldNotReturnFractionPeriodWhenStartAndEndOnInterval()
		{
			var visualLayer = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 45, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc)),
											  activity, person);

			var layers = new List<IVisualLayer> { visualLayer };
			var visualLayers = new VisualLayerCollection(person, layers, new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15).ToArray();

			result.Single().FractionPeriod.HasValue.Should().Be.False();
		}
	}
}