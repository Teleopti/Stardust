using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface IOverridePersister
	{
		void Persist(IScenario scenario, IWorkload workload, OverrideInput input);
	}
}