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

		public DefaultDateCalculator(INow now)
		{
			_now = now;
		}

		public DateOnly Calculate(IWorkflowControlSet workflowControlSet, Func<IWorkflowControlSet, DateOnlyPeriod> periodInWorkflowControlSet, IList<IPersonPeriod> personPeriods)
		{
			if (workflowControlSet == null)
				return _now.ServerDate_DontUse();
			if (!personPeriods.Any())
				return _now.ServerDate_DontUse();
			var workflowControlSetPeriod = periodInWorkflowControlSet.Invoke(workflowControlSet);
			var minPersonPeriodStart = personPeriods.Min(p => p.StartDate);

			if (workflowControlSetPeriod.Contains(minPersonPeriodStart) && _now.ServerDate_DontUse() < workflowControlSetPeriod.EndDate)
			{
				var personPeriod = personPeriods.FirstOrDefault(pp => _now.ServerDate_DontUse() < pp.StartDate );
				return personPeriod == null ? _now.ServerDate_DontUse() : personPeriod.StartDate;
			}

			return workflowControlSetPeriod.StartDate < _now.ServerDate_DontUse() || workflowControlSetPeriod.EndDate<minPersonPeriodStart ?
							 _now.ServerDate_DontUse() :
							 workflowControlSetPeriod.StartDate;
		}
	}
}