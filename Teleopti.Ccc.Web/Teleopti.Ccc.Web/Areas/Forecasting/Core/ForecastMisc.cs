namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class ForecastMisc : IForecastMisc
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