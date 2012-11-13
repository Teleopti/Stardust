using System;

namespace Teleopti.Ccc.Sdk.LoadTestClient
{
	public interface ITestScenario {
		void RunOnce();
		void WriteStatus(Action<string> writer);
		TimeSpan LastElapsedTime { get; }
	}
}