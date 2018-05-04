using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface ICampaignPersister
	{
		void Persist(IScenario scenario, IWorkload workload, DateOnly[] days, double campaignTasksPercent);
	}
}