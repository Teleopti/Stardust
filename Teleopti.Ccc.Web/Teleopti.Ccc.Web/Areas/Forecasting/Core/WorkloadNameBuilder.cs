namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class WorkloadNameBuilder : IWorkloadNameBuilder
	{
		public string WorkloadName(string skillName, string workloadName)
		{
			if (string.IsNullOrEmpty(workloadName) || workloadName == skillName)
			{
				return skillName;
			}
			return skillName + " - " + workloadName;
		}
	}
}