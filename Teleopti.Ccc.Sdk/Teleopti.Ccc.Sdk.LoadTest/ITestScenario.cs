using System;

namespace Teleopti.Ccc.Sdk.LoadTest
{
	public interface ITestScenario {
		void RunOnce();
		void WriteStatus(Action<string> writer);
		TimeSpan LastElapsedTime { get; }
	}
}