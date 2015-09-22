using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public interface ICampaignPersister
	{
		void Persist(IScenario scenario, IWorkload workload, CampaignDay[] days, int campaignTasksPercent);
	}
}