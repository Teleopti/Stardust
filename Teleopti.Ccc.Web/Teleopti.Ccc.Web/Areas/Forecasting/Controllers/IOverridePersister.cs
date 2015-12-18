using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public interface IOverridePersister
	{
		void Persist(IScenario scenario, IWorkload workload, OverrideInput input);
	}
}