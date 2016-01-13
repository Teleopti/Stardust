using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface ICampaignPersister
	{
		void Persist(IScenario scenario, IWorkload workload, ModifiedDay[] days, double campaignTasksPercent);
	}
}