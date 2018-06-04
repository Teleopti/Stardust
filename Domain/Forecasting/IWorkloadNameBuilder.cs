namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface IWorkloadNameBuilder
	{
		string WorkloadName(string skillName, string workloadName);
	}
}