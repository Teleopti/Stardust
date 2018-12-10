using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.SeatManagement;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public interface ISeatBookingReportProvider
	{
		SeatBookingReportViewModel Get(SeatBookingReportCriteria criteria, Paging paging = new Paging());
		SeatBookingReportViewModel Get(SeatBookingReportCommand command);
		SeatBookingSummary GetSummary (DateOnly date);
	}
}