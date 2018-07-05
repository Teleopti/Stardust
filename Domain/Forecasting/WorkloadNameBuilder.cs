namespace Teleopti.Ccc.Domain.Forecasting
{
	public class WorkloadNameBuilder
	{
		public static string GetWorkloadName(string skillName, string workloadName)
		{
			if (string.IsNullOrEmpty(workloadName) || workloadName == skillName)
			{
				return skillName;
			}
			return skillName + " - " + workloadName;
		}
	}
}