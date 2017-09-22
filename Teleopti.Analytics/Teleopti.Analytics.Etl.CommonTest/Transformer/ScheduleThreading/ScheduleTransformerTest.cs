using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Analytics.Etl.CommonTest.Transformer.Job;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.ScheduleThreading
{
	[TestFixture]
	public class ScheduleTransformerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void Verify()
		{
			var scheduleFactory = new ScheduleFactory();
			IList<IScheduleDay> scheduleCollection = scheduleFactory.CreateShiftCollection();
			const int intervalsPerDay = 288;
			const int minutesPerInterval = 1440 / intervalsPerDay;

			IJobParameters jobParameters = new JobParameters(
				JobMultipleDateFactory.CreateJobMultipleDate(), 1,
				"W. Europe Standard Time",
				minutesPerInterval,
				"Data Source=SSAS_Server;Initial Catalog=SSAS_DB", "false",
				CultureInfo.CurrentCulture, 
				new JobParametersFactory.FakeContainerHolder(), 
				false);

			jobParameters.Helper = new JobHelperForTest(new RaptorRepositoryForTest(), null);
			
			var transformer = new ScheduleTransformer();   

			var mockRepository = new MockRepository();
			var threadPool = mockRepository.DynamicMock<IThreadPool>();
			mockRepository.ReplayAll();
			transformer.Transform(scheduleCollection, new DateTime(), jobParameters, threadPool);
		}
	}
}
