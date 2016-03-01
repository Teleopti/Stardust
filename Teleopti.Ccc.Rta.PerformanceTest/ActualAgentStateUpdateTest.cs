using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("ActualAgentStateUpdateTest")]
	[RtaPerformanceTest]
	public class ActualAgentStateUpdateTest
	{
		public Database Database;
		public RtaStates RtaStates;
		public IAgentStateReadModelReader AgentStateReader;
		
		[Test]
		public void MeasurePerformance()
		{
			var watch = Stopwatch.StartNew();
			Database.Setup();
			Console.WriteLine(watch.Elapsed);

			RtaStates.Send();
			Console.WriteLine(watch.Elapsed);

			waitForAllStatesArePersisted();
			Console.WriteLine(watch.Elapsed);
		}

		private void waitForAllStatesArePersisted()
		{
			while (true)
			{
				if (AgentStateReader.GetActualAgentStates().All(x => x.ReceivedTime == "2016-02-26 17:05".Utc()))
					break;
				Thread.Sleep(20);
			}
		}
	}
}