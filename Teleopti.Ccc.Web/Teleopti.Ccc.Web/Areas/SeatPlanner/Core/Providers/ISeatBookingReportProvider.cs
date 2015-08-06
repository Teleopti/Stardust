using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public interface ISeatBookingReportProvider
	{
		SeatBookingReportViewModel Get(SeatBookingReportCriteria criteria, Paging paging = null);
		SeatBookingReportViewModel Get(SeatBookingReportCommand command);
	}
}