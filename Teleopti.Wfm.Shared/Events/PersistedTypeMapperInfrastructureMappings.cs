using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	internal static class PersistedTypeMapperInfrastructureMappings
	{
		public static IEnumerable<PersistedTypeMapping> Mappings()
		{
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireEventJob",
				LegacyPersistedNames = new[]
				{
					"Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Ccc.Infrastructure",
					"Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Wfm.Shared"
				},
				CurrentTypeName = "Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventJob, Teleopti.Wfm.Shared"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireEventServer",
				LegacyPersistedNames = new[]
				{
					"Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventServer, Teleopti.Ccc.Infrastructure",
					"Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventServer, Teleopti.Wfm.Shared"
				},
				CurrentTypeName = "Teleopti.Ccc.Infrastructure.Hangfire.HangfireEventServer, Teleopti.Ccc.Infrastructure"
			};
		}
	}
}