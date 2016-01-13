namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public interface IForecastMisc
	{
		string WorkloadName(string skillName, string workloadName);
	}

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