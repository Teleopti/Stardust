namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface IWorkloadNameBuilder
	{
		string WorkloadName(string skillName, string workloadName);
	}
}