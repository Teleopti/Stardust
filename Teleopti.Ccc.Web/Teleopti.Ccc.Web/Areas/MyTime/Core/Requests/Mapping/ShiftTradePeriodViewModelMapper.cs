using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradePeriodViewModelMapper : IShiftTradePeriodViewModelMapper
	{
		public ShiftTradeRequestsPeriodViewModel Map(IWorkflowControlSet workflowControlSet, INow now, TimeZoneInfo timeZone)
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
			var agentTime = now.CurrentLocalDateTime(timeZone);
			vm.NowYear = calendar.GetYear(agentTime);
			vm.NowMonth = calendar.GetMonth(agentTime);
			vm.NowDay = calendar.GetDayOfMonth(agentTime);

			return vm;
		}
	}
}