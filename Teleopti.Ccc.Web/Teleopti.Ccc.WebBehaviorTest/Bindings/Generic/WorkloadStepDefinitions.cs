using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class WorkloadStepDefinitions
	{
		[Given(@"there is a Workload '(.*)' with Skill '(.*)' and queuesource '(.*)'")]
		public void GivenThereIsAWorkloadWithSkillAndQueuesource(string workloadName, string skill, string queueSource)
		{
			DataMaker.Data().Apply(new WorkloadConfigurable
			{
				WorkloadName = workloadName,
				SkillName = skill,
				QueueSourceName = queueSource,
				Open24Hours = true
			});
		}

	}

	[Binding]
	public class QueueSourceDefinitions
	{
		[Given(@"there is a QueueSource named '(.*)'")]
		public void GivenThereIsAQueueSourceNamed(string queueSource)
		{
			DataMaker.Data().Apply(new QueueSourceConfigurable
			{
				Name = queueSource
			});
		}

		[Given(@"there is queue statistics for '(.*)'")]
		public void GivenThereIsQueueStatisticsFor(string name)
		{
			var timeZones = new UtcAndCetTimeZones();
			var intervals = new QuarterOfAnHourInterval();
			var datasource = new ExistingDatasources(timeZones);
			
			//common analytics data
			DataMaker.Data().Analytics().Setup(new EternityAndNotDefinedDate());
			DataMaker.Data().Analytics().Setup(timeZones);

			DataMaker.Data().Analytics().Setup(intervals);
			DataMaker.Data().Analytics().Setup(datasource);
			var days = new List<Tuple<SpecificDate,IBridgeTimeZone>>();
			for (var i = 0; i < 20; i++)
			{
				var date = new DateOnly(i + 2013, 1, 1);
				var theDay = new SpecificDate {Date = new DateOnly(date), DateId = i};
				DataMaker.Data().Analytics().Setup(theDay);
				var bridgeTimeZone = new FillBridgeTimeZoneFromData(theDay, intervals, timeZones, datasource);
				DataMaker.Data().Analytics().Setup(bridgeTimeZone);
				days.Add(new Tuple<SpecificDate, IBridgeTimeZone>(theDay, bridgeTimeZone));
			}

			var queueDataSource = DataMaker.Data().UserData<IDatasourceData>();
			const int queueId = 5;
			var queue = new AQueue(queueDataSource) { QueueId = queueId };
			DataMaker.Data().Analytics().Setup(queue);

			foreach (var day in days)
			{
				DataMaker.Data().Analytics().Setup(new FactQueue(day.Item1, intervals, queue, queueDataSource, day.Item2));
				
			}

			DataMaker.Data().Apply(new QueueSourceConfigurable
			{
				Name = name,
				QueueId = queueId
			});
		}

	}
}