using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class AnalyticsWorkloadFactory
	{
		public static AnalyticsWorkload CreateAnalyticsWorkload(IWorkload workload)
		{
			return CreateAnalyticsWorkload(workload, 1, 1);
		}

		public static AnalyticsWorkload CreateAnalyticsWorkload(IWorkload workload, int skillId, int workloadId)
		{
			return new AnalyticsWorkload
			{
				WorkloadCode = workload.Id.GetValueOrDefault(),
				WorkloadId = workloadId,
				SkillCode = workload.Skill.Id.GetValueOrDefault(),
				SkillId = skillId,
			};
		}
	}
}