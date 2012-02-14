using System.Diagnostics;

namespace Teleopti.Ccc.Sdk.LoadTest
{
	public static class Counters
	{
		public static PerformanceCounter RunningScenarios;
		public static PerformanceCounter AverageScenarioTime;
		public static PerformanceCounter AverageScenarioTimeBase;

		static Counters()
		{
			if (PerformanceCounterCategory.Exists("SdkLoadTest"))
				PerformanceCounterCategory.Delete("SdkLoadTest");

			var counters = new CounterCreationDataCollection();
			counters.AddRange(new[] {
			                        	new CounterCreationData {
			                        	                        	CounterType = PerformanceCounterType.NumberOfItems32,
			                        	                        	CounterName = "Running Scenarios",
			                        	                        	CounterHelp = "Count of current running scenarios"
			                        	                        },
			                        	new CounterCreationData {
			                        	                        	CounterType = PerformanceCounterType.AverageTimer32, 
			                        	                        	CounterName = "Average Scenario Time",
			                        	                        	CounterHelp = "Average scenario time in seconds"
			                        	                        },
			                        	new CounterCreationData {
			                        	                        	CounterType = PerformanceCounterType.AverageBase, 
			                        	                        	CounterName = "Average Scenario Time Base"
																}
			                        });

			PerformanceCounterCategory.Create("SdkLoadTest", "", PerformanceCounterCategoryType.SingleInstance, counters);

			RunningScenarios = new PerformanceCounter("SdkLoadTest", "Running Scenarios", false);
			AverageScenarioTime = new PerformanceCounter("SdkLoadTest", "Average Scenario Time", false);
			AverageScenarioTimeBase = new PerformanceCounter("SdkLoadTest", "Average Scenario Time Base", false);

			RunningScenarios.RawValue = 0;
			AverageScenarioTime.RawValue = 0;
			AverageScenarioTimeBase.RawValue = 0;
		}



	}
}