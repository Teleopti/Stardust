using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public interface ISeatPlanProvider
	{
		SeatPlanViewModel Get (DateOnly date);
		List<SeatPlanViewModel> Get (DateOnlyPeriod dateOnlyPeriod);
	}
}