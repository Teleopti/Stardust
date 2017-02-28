using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOverrideTask : IEquatable<IOverrideTask>
	{
		double? OverrideTasks { get; }

		TimeSpan? OverrideAverageTaskTime { get; }

		TimeSpan? OverrideAverageAfterTaskTime { get; }
	}
}