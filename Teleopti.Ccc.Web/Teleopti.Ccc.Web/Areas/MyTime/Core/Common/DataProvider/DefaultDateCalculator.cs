using System;
using System.Collections.Generic;
using System.Linq;
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

			var returnDate = workflowControlSetPeriod.StartDate < _now.LocalDateOnly()
				                 ? _now.LocalDateOnly()
				                 : workflowControlSetPeriod.StartDate;
			var personPeriodStartingInWcsPeriod =
				personPeriods.FirstOrDefault(pp => pp.StartDate > workflowControlSetPeriod.StartDate && pp.StartDate > _now.LocalDateOnly());
			return personPeriodStartingInWcsPeriod == null ? returnDate : personPeriodStartingInWcsPeriod.StartDate;
		}
	}
}