using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class OpenShiftTradePeriodSpecification : Specification<ShiftTradeAvailableCheckItem>, IShiftTradeLightSpecification
	{
		private readonly INow _now;

		public OpenShiftTradePeriodSpecification(INow now)
		{
			_now = now;
		}

		public override bool IsSatisfiedBy(ShiftTradeAvailableCheckItem obj)
		{
			if (obj.PersonFrom.WorkflowControlSet == null || obj.PersonTo.WorkflowControlSet == null)
				return false;
			var personFromToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), obj.PersonFrom.PermissionInformation.DefaultTimeZone()).Date);
			var personToToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), obj.PersonTo.PermissionInformation.DefaultTimeZone()).Date);
			var openPeriodFrom =
				 new DateOnlyPeriod(
					  personFromToday.AddDays(obj.PersonFrom.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum),
					  personFromToday.AddDays(obj.PersonFrom.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum));
			var openPeriodTo =
				 new DateOnlyPeriod(
					  personToToday.AddDays(obj.PersonTo.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum),
					  personToToday.AddDays(obj.PersonTo.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum));
			return openPeriodFrom.Contains(obj.DateOnly) && openPeriodTo.Contains(obj.DateOnly);
		}

		public string DenyReason => nameof(Resources.OpenShiftTradePeriodDenyReason);
	}
}
