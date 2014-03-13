using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class DefaultDateCalculator : IDefaultDateCalculator
	{
		public DateOnly Calculate(IWorkflowControlSet workflowControlSet, Func<IWorkflowControlSet, DateOnlyPeriod> periodInWorkflowControlSet, IList<IPersonPeriod> personPeriods)
		{
			if (workflowControlSet == null)
				return DateOnly.Today;
			if (!personPeriods.Any())
				return DateOnly.Today;
			var workflowControlSetPeriod = periodInWorkflowControlSet.Invoke(workflowControlSet);

			var returnDate = workflowControlSetPeriod.StartDate < DateOnly.Today
				                 ? DateOnly.Today
				                 : workflowControlSetPeriod.StartDate;
			var personPeriodStartingInWcsPeriod =
				personPeriods.FirstOrDefault(pp => pp.StartDate > workflowControlSetPeriod.StartDate && pp.StartDate > DateOnly.Today);
			return personPeriodStartingInWcsPeriod == null ? returnDate : personPeriodStartingInWcsPeriod.StartDate;
		}
	}
}