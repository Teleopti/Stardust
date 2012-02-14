using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class ProjectionIntersectingPeriodMergerTest
	{
		private IProjectionMerger target;
		private IActivity activity;
		private IPerson person;
		private IVisualLayerFactory visualLayerFactory;

		[SetUp]
		public void Setup()
		{
			target = new ProjectionIntersectingPeriodMerger();
			activity = new Activity("f");
			person = new Person();
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
			             	};
			Assert.AreEqual(2, target.MergedCollection(layers, person).Count);
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
			             		createLayer(new DateTimePeriod(2014, 1, 1, 2015, 1, 1)),
			             	};

			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2003, 1, 1), target.MergedCollection(layers, person).First().Period);
			Assert.AreEqual(new DateTimePeriod(2010, 1, 1, 2015, 1, 1), target.MergedCollection(layers, person).Last().Period);
		}

		private IVisualLayer createLayer(DateTimePeriod period)
		{
			return visualLayerFactory.CreateShiftSetupLayer(activity, period);
		}
	}
}