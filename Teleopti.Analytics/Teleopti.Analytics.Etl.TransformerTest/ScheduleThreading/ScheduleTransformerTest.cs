using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.ScheduleThreading
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

			IJobParameters jobParameters = new JobParameters(JobMultipleDateFactory.CreateJobMultipleDate(), 1,
															 "W. Europe Standard Time",
															 minutesPerInterval,
															 "Data Source=SSAS_Server;Initial Catalog=SSAS_DB", "false",
															 CultureInfo.CurrentCulture);

			jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), null, null, null);
			
			var transformer = new ScheduleTransformer();   

			var mockRepository = new MockRepository();
			var threadPool = mockRepository.DynamicMock<IThreadPool>();
			mockRepository.ReplayAll();
			transformer.Transform(scheduleCollection, new DateTime(), jobParameters, threadPool);
		}
	}
}
