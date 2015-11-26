using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public interface IOverridePersister
	{
		void Persist(IScenario scenario, IWorkload workload, ModifiedDay[] days, double? overrideTasks, TimeSpan? overrideTalkTime, TimeSpan? overrideAfterCallWork);
	}
}