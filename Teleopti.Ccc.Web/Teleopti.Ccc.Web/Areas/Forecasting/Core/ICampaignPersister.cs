using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface ICampaignPersister
	{
		void Persist(IScenario scenario, IWorkload workload, ModifiedDay[] days, double campaignTasksPercent);
	}
}