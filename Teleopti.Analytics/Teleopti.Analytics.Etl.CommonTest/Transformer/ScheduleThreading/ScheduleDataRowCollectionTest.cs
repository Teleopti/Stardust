using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Analytics.Etl.CommonTest.Transformer.Job;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.ScheduleThreading
{
	[TestFixture]
	public class ScheduleDataRowCollectionTest
	{
		private IJobParameters _jobParameters;

		[SetUp]
		public void Setup()
		{
			_jobParameters = JobParametersFactory.SimpleParameters(false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void Verify()
		{
			IList<IScheduleDay> scheduleList = SchedulePartFactory.CreateSchedulePartCollection();
			IList<ScheduleProjection> scheduleProjectionServiceList = ProjectionsForAllAgentSchedulesFactory.CreateProjectionsForAllAgentSchedules(scheduleList);

			var scheduleProjections = new List<ScheduleProjection> { scheduleProjectionServiceList[0] };

			_jobParameters.Helper = new JobHelperForTest(new RaptorRepositoryForTest(), null);

			IVisualLayerCollection layerCollection = VisualLayerCollectionFactory.CreateForWorkShift(new Person(),
																									 new TimeSpan(9, 0, 0),
																									 new TimeSpan(16, 0, 0),
																									 new TimePeriod(11, 0, 12, 0));
			IVisualLayer layer = layerCollection.First();
			var layerCollectionWIthFirstLayer = new FilteredVisualLayerCollection(new Person(), new List<IVisualLayer> { layer }, new ProjectionIntersectingPeriodMerger(), null);
			

			var intervalBase = new IntervalBase(new DateTime(), 288);

			var scheduleDataRowFactory = new ScheduleDataRowFactory();

			using (var table = new DataTable())
			{
				table.Locale = Thread.CurrentThread.CurrentCulture;
				ScheduleInfrastructure.AddColumnsToDataTable(table);
				scheduleDataRowFactory.CreateScheduleDataRow(table, layer, scheduleProjections[0], intervalBase, DateTime.Now, 288,
				                                             layerCollectionWIthFirstLayer);
			}
		}
	}
}
