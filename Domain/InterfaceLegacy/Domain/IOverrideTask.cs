using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IOverrideTask : IEquatable<IOverrideTask>
	{
		double? OverrideTasks { get; }

		TimeSpan? OverrideAverageTaskTime { get; }

		TimeSpan? OverrideAverageAfterTaskTime { get; }
	}
}