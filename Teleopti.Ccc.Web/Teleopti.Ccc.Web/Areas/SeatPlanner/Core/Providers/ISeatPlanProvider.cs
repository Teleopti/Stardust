using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public interface ISeatPlanProvider
	{
		SeatPlanViewModel Get (DateOnly date);
		List<SeatPlanViewModel> Get (DateOnlyPeriod dateOnlyPeriod);
	}
}