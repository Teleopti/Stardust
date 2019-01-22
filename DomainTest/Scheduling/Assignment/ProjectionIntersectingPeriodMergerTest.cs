using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class ProjectionIntersectingPeriodMergerTest
	{
		private IProjectionMerger target;
		private IActivity activity;
		private IVisualLayerFactory visualLayerFactory;

		[SetUp]
		public void Setup()
		{
			target = new ProjectionIntersectingPeriodMerger();
			activity = ActivityFactory.CreateActivity("f");
			visualLayerFactory = new VisualLayerFactory();
		}

		[Test]
		public void VerifyAssignedPeriodCollectionCount()
		{
			var layers = new List<IVisualLayer>
			             	{
			             		createLayer(new DateTimePeriod(2000, 1, 1, 2001, 1, 1)),
			             		createLayer(new DateTimePeriod(2001, 1, 1, 2002, 1, 1)),
			             		createLayer(new DateTimePeriod(2004, 1, 1, 2005, 1, 1))
			             	}.ToArray();
			Assert.AreEqual(2, target.MergedCollection(layers).Count());
		}

		[Test]
		public void VerifyAssignedPeriodCollectionPeriod()
		{
			var layers = new List<IVisualLayer>
			             	{
			             		createLayer(new DateTimePeriod(2000, 1, 1, 2001, 1, 1)),
			             		createLayer(new DateTimePeriod(2001, 1, 1, 2002, 1, 1)),
			             		createLayer(new DateTimePeriod(2002, 1, 1, 2003, 1, 1)),

								createLayer(new DateTimePeriod(2010, 1, 1, 2011, 1, 1)),
			             		createLayer(new DateTimePeriod(2011, 1, 1, 2012, 1, 1)),
			             		createLayer(new DateTimePeriod(2012, 1, 1, 2013, 1, 1)),
			             		createLayer(new DateTimePeriod(2013, 1, 1, 2014, 1, 1)),
			             		createLayer(new DateTimePeriod(2014, 1, 1, 2015, 1, 1))
			             	}.ToArray();

			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2003, 1, 1), target.MergedCollection(layers).First().Period);
			Assert.AreEqual(new DateTimePeriod(2010, 1, 1, 2015, 1, 1), target.MergedCollection(layers).Last().Period);
		}

		private IVisualLayer createLayer(DateTimePeriod period)
		{
			return visualLayerFactory.CreateShiftSetupLayer(activity, period);
		}
	}
}