using System.Globalization;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradePeriodViewModelMapper : IShiftTradePeriodViewModelMapper
	{
		public ShiftTradeRequestsPeriodViewModel Map(IWorkflowControlSet workflowControlSet, INow now)
		{
			var vm = new ShiftTradeRequestsPeriodViewModel { HasWorkflowControlSet = workflowControlSet != null };

			if (workflowControlSet != null)
			{
				vm.OpenPeriodRelativeStart = workflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum;
				vm.OpenPeriodRelativeEnd = workflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum;
			}

			var calendar = CultureInfo.CurrentCulture.Calendar;
			vm.NowYear = calendar.GetYear(now.LocalDateOnly());
			vm.NowMonth = calendar.GetMonth(now.LocalDateOnly());
			vm.NowDay = calendar.GetDayOfMonth(now.LocalDateOnly());

			return vm;
		}
	}
}