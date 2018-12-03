using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Helper
{
	[TestFixture]
	public class VisualLayerCollectionExtensionsTest
	{
		private readonly IActivity activity = ActivityFactory.CreateActivity("Phone");
		private readonly IActivity secondActivity = ActivityFactory.CreateActivity("Break");

		[Test]
		public void ShouldReturnFractionOfResourceWhenStartWithinIntervalWithTwoLayers()
		{
			var visualLayer1 = new VisualLayer(secondActivity, new DateTimePeriod(new DateTime(2001, 1, 1, 9, 45, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 10, 12, 0, DateTimeKind.Utc)),
											  secondActivity);
			var visualLayer2 = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 12, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 10, 15, 0, DateTimeKind.Utc)),
											  activity);
			var visualLayer3 = new VisualLayer(secondActivity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 15, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 10, 30, 0, DateTimeKind.Utc)),
											  secondActivity);

			var layers = new List<IVisualLayer> { visualLayer1, visualLayer2, visualLayer3 };
			var visualLayers = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15, TimeZoneInfo.Utc).ToArray();

			result.First(r => r.Period.StartDateTime == new DateTime(2001, 1, 1, 10, 0, 0, DateTimeKind.Utc)).Resource.Should().Be.EqualTo(0.8);
			result.Last(r => r.Period.StartDateTime == new DateTime(2001, 1, 1, 10, 0, 0, DateTimeKind.Utc)).Resource.Should().Be.EqualTo(0.2);
		}
	
		[Test]
		public void ShouldReturnFractionOfResourceWhenStartWithinInterval()
		{
			var visualLayer = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 12, 0, DateTimeKind.Utc),
			                                                               new DateTime(2001, 1, 1, 10, 15, 0, DateTimeKind.Utc)),
			                                  activity);
			
			var layers = new List<IVisualLayer> { visualLayer };
			var visualLayers = new VisualLayerCollection(layers,new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15, TimeZoneInfo.Utc).ToArray();

			var resultLayer = result.Single();
			resultLayer.Resource.Should().Be.EqualTo(0.2);
			resultLayer.FractionPeriod.Value.Should().Be.EqualTo(visualLayer.Period);
		}

		[Test]
		public void ShouldReturnFractionOfResourceWhenEndWithinInterval()
		{
			var visualLayer = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 45, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 10, 48, 0, DateTimeKind.Utc)),
											  activity);

			var layers = new List<IVisualLayer> { visualLayer };
			var visualLayers = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15, TimeZoneInfo.Utc).ToArray();

			var resultLayer = result.Single();
			resultLayer.Resource.Should().Be.EqualTo(0.2);
			resultLayer.FractionPeriod.Value.Should().Be.EqualTo(visualLayer.Period);
		}

		[Test]
		public void ShouldReturnFractionOfResourceWhenBothStartAndEndWithinInterval()
		{
			var visualLayer = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 48, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 10, 51, 0, DateTimeKind.Utc)),
											  activity);

			var layers = new List<IVisualLayer> { visualLayer };
			var visualLayers = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15, TimeZoneInfo.Utc).ToArray();

			var resultLayer = result.Single();
			resultLayer.Resource.Should().Be.EqualTo(0.2);
			resultLayer.FractionPeriod.Value.Should().Be.EqualTo(visualLayer.Period);
		}

		[Test]
		public void ShouldReturnResourceWhenStartAndEndOnInterval()
		{
			var visualLayer = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 45, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc)),
											  activity);

			var layers = new List<IVisualLayer> { visualLayer };
			var visualLayers = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15, TimeZoneInfo.Utc).ToArray();

			result.Single().Resource.Should().Be.EqualTo(1.0);
		}

		[Test]
		public void ShouldNotReturnFractionPeriodWhenStartAndEndOnInterval()
		{
			var visualLayer = new VisualLayer(activity, new DateTimePeriod(new DateTime(2001, 1, 1, 10, 45, 0, DateTimeKind.Utc),
																		   new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc)),
											  activity);

			var layers = new List<IVisualLayer> { visualLayer };
			var visualLayers = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15, TimeZoneInfo.Utc).ToArray();

			result.Single().FractionPeriod.HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldNotAddResourceLayerOutsideProvidedLayersWhenLayersNotAdjacent()
		{
			var dateTimePeriod1 = new DateTimePeriod(new DateTime(2001, 1, 1, 10, 45, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc));
			var dateTimePeriod2 = new DateTimePeriod(new DateTime(2001, 1, 1, 12, 45, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 13, 0, 0, DateTimeKind.Utc));

			var visualLayer1 = new VisualLayer(activity, dateTimePeriod1 , activity);
			var visualLayer2 = new VisualLayer(activity, dateTimePeriod2, activity);

			var layers = new List<IVisualLayer> {visualLayer1, visualLayer2};
			var visualLayers = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

			var result = visualLayers.ToResourceLayers(15, TimeZoneInfo.Utc).ToArray();
			Assert.AreEqual(2, result.Count());
			Assert.IsTrue(result[0].Period.Equals(dateTimePeriod1));
			Assert.IsTrue(result[1].Period.Equals(dateTimePeriod2));
		}
	}
}