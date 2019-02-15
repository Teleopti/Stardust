using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Data;


namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class QueueSourceDefinitions
	{
		[Given(@"there is no queue statistics for '(.*)'")]
		public void GivenThereIsNoQueueStatisticsFor(string queue)
		{
			//Step definition for testers to make sure there is no statistics
			DataMaker.Data().Apply(new QueueSourceConfigurable
			{
				Name = queue
			});
		}
		
		[Given(@"there is queue statistics for '(.*)'")]
		public void GivenThereIsQueueStatisticsFor(string name)
		{
			var intervalData = DefaultAnalyticsDataCreator.GetInterval();
			var timeZoneData = DefaultAnalyticsDataCreator.GetTimeZoneRows();
			var datasourceData = DefaultAnalyticsDataCreator.GetDataSources();

			var days = new List<Tuple<SpecificDate, IBridgeTimeZone>>();
			for (var i = 0; i < 20; i++)
			{
				var date = new DateOnly(i + 2013, 1, 1);
				var theDay = new SpecificDate
				{
					Date = date,
					DateId = i,
					Rows = new[] {DefaultAnalyticsDataCreator.GetDateRow(date.Date)}
				};
				var bridgeTimeZone = new FillBridgeTimeZoneFromData(theDay, intervalData, timeZoneData, datasourceData);
				DataMaker.Data().Analytics().Setup(bridgeTimeZone);
				days.Add(new Tuple<SpecificDate, IBridgeTimeZone>(theDay, bridgeTimeZone));
			}

			const int queueId = 5;
			var queue = new AQueue(datasourceData) { QueueId = queueId };
			DataMaker.Data().Analytics().Apply(queue);

			foreach (var day in days)
			{
				DataMaker.Data().Analytics().Setup(new FactQueue(day.Item1, intervalData, queue, datasourceData, day.Item2, 96));
			}

			DataMaker.Data().Apply(new QueueSourceConfigurable
			{
				Name = name,
				QueueId = queueId
			});
		}

	}
}