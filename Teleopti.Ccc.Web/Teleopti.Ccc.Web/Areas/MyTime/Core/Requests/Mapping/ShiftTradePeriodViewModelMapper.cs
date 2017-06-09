using System.Globalization;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradePeriodViewModelMapper : IShiftTradePeriodViewModelMapper
	{
		public ShiftTradeRequestsPeriodViewModel Map(IWorkflowControlSet workflowControlSet, INow now)
		{
			var vm = new ShiftTradeRequestsPeriodViewModel();

			if (workflowControlSet != null)
			{
				vm.HasWorkflowControlSet = true;
				vm.MiscSetting = new ShiftTradeRequestMiscSetting()
				{
					AnonymousTrading = workflowControlSet.AnonymousTrading
				};
				vm.OpenPeriodRelativeStart = workflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum;
				vm.OpenPeriodRelativeEnd = workflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum;
			}

			var calendar = CultureInfo.CurrentCulture.Calendar;
			vm.NowYear = calendar.GetYear(now.ServerDateTime_DontUse());
			vm.NowMonth = calendar.GetMonth(now.ServerDateTime_DontUse());
			vm.NowDay = calendar.GetDayOfMonth(now.ServerDateTime_DontUse());

			return vm;
		}
	}
}