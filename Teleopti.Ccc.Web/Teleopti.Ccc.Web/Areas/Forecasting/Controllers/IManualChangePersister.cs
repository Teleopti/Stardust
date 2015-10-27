using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public interface IManualChangePersister
	{
		void Persist(IScenario scenario, IWorkload workload, ModifiedDay[] days, int manualChangeValue);
	}
}