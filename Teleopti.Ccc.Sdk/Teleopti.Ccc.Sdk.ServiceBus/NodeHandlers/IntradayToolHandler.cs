using System;
using System.Collections.Generic;
using System.Threading;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Intraday.TestCommon;
using Teleopti.Ccc.Intraday.TestCommon.TestApplication;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class IntradayToolHandler : IHandle<IntradayToolEvent>
	{
		private readonly IForecastProvider forecastProvider;
		private readonly IWorkloadQueuesProvider workloadQueuesProvider;
		private readonly IDictionary<int, IList<QueueInterval>> queueDataDictionary;
		private readonly IQueueDataPersister queueDataPersister;
		private readonly TimeZoneprovider timeZoneprovider;
		private readonly UserTimeZoneProvider userTimeZoneProvider;
		private readonly UniqueQueueProvider uniqueQueueProvider;

		public IntradayToolHandler(IConfigReader configReader = null)
		{
			var configReader1 = configReader ?? new ConfigReader();

			var analyticsConnectionString = configReader1.ConnectionString("Hangfire");
			var appDbConnectonString = configReader1.ConnectionString("Tenancy");
			forecastProvider = new ForecastProvider(analyticsConnectionString);
			workloadQueuesProvider = new WorkloadQueuesProvider(analyticsConnectionString);
			queueDataDictionary = new Dictionary<int, IList<QueueInterval>>();
			queueDataPersister = new QueueDataPersister(analyticsConnectionString);
			timeZoneprovider = new TimeZoneprovider(analyticsConnectionString);
			userTimeZoneProvider = new UserTimeZoneProvider(appDbConnectonString);
			uniqueQueueProvider = new UniqueQueueProvider(appDbConnectonString, analyticsConnectionString);
		}

		public void Handle(IntradayToolEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress, ref IEnumerable<object> returnObjects)
		{
			var generateIntradayData = new GenerateIntradayData(workloadQueuesProvider, forecastProvider, uniqueQueueProvider,
				queueDataDictionary, queueDataPersister, userTimeZoneProvider, timeZoneprovider);

			var userTimeZoneInfo = new UserTimeZoneInfo("", "0");

			generateIntradayData.GenerateData(generateIntradayData.GenerateInput(userTimeZoneInfo), false);
		}
	}
}
