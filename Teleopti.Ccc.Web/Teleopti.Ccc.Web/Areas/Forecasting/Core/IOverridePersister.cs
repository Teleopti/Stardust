using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface IOverridePersister
	{
		void Persist(IScenario scenario, IWorkload workload, OverrideInput input);
	}
}