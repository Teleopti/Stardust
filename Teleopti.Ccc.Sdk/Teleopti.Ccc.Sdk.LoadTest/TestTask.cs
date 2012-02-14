using System;
using System.Diagnostics;
using System.Threading;

namespace Teleopti.Ccc.Sdk.LoadTest
{
	public class TestTask
	{
		private readonly TimeSpan _delayTime;
		private readonly TimeSpan _intervalTime;
		private readonly ITestScenario _scenario;
		private bool _stop;

		public TestTask(TimeSpan delayTime, TimeSpan intervalTime, ITestScenario scenario)
		{
			_delayTime = delayTime;
			_intervalTime = intervalTime;
			_scenario = scenario;
		}

		public void StartAsync()
		{
			var thread = new Thread(ThreadMethod);
			thread.Start();
		}

		public void StopAsync()
		{
			_stop = true;
		}

		public void ThreadMethod()
		{
			Thread.Sleep(_delayTime);
			while (!_stop)
			{
				var stopWatch = new Stopwatch();
				stopWatch.Start();
				//Counters.RunningScenarios.Increment();

				_scenario.RunOnce();

				stopWatch.Stop();
				//Counters.AverageScenarioTimeBase.Increment();
				//Counters.AverageScenarioTime.IncrementBy(stopWatch.ElapsedTicks);
				//Counters.RunningScenarios.Decrement();

				Thread.Sleep(_intervalTime);
			}
		}

		public void WriteStatus(Action<string> writer)
		{
			_scenario.WriteStatus(writer);
		}
	}
}