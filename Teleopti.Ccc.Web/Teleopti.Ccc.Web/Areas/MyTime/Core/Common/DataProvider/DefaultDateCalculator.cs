using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Ajax.Utilities;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;

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
				return _now.LocalDateOnly();
			if (!personPeriods.Any())
				return _now.LocalDateOnly();
			var workflowControlSetPeriod = periodInWorkflowControlSet.Invoke(workflowControlSet);
			var minPersonPeriodStart = personPeriods.Min(p => p.StartDate);

			if (workflowControlSetPeriod.Contains(minPersonPeriodStart) && _now.LocalDateOnly() < workflowControlSetPeriod.EndDate)
			{
				var personPeriod = personPeriods.FirstOrDefault(pp => _now.LocalDateOnly() < pp.StartDate );
				return personPeriod == null ? _now.LocalDateOnly() : personPeriod.StartDate;
			}

			return workflowControlSetPeriod.StartDate < _now.LocalDateOnly() || workflowControlSetPeriod.EndDate<minPersonPeriodStart ?
							 _now.LocalDateOnly() :
							 workflowControlSetPeriod.StartDate;
		}
	}
}