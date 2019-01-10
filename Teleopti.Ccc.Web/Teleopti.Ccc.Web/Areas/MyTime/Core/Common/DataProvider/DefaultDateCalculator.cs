using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class DefaultDateCalculator : IDefaultDateCalculator
	{
		private readonly INow _now;
		private readonly IUserTimeZone _userTimeZone;

		public DefaultDateCalculator(INow now, IUserTimeZone userTimeZone)
		{
			_now = now;
			_userTimeZone = userTimeZone;
		}

		public DateOnly Calculate(IWorkflowControlSet workflowControlSet, Func<IWorkflowControlSet, DateOnlyPeriod> periodInWorkflowControlSet, IList<IPersonPeriod> personPeriods)
		{
			var today = _now.CurrentLocalDate(_userTimeZone.TimeZone());
			if (workflowControlSet == null)
				return today;
			if (!personPeriods.Any())
				return today;
			var workflowControlSetPeriod = periodInWorkflowControlSet.Invoke(workflowControlSet);
			var minPersonPeriodStart = personPeriods.Min(p => p.StartDate);

			if (workflowControlSetPeriod.Contains(minPersonPeriodStart) && today < workflowControlSetPeriod.EndDate)
			{
				var personPeriod = personPeriods.FirstOrDefault(pp => today < pp.StartDate );
				return personPeriod?.StartDate ?? today;
			}

			return workflowControlSetPeriod.StartDate < today || workflowControlSetPeriod.EndDate<minPersonPeriodStart ? today : workflowControlSetPeriod.StartDate;
		}
	}
}