using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class DefaultDateCalculator : IDefaultDateCalculator
	{
		public DateOnly Calculate(IWorkflowControlSet workflowControlSet, Func<IWorkflowControlSet, DateOnlyPeriod> periodInWorkflowControlSet)
		{
			if (workflowControlSet == null)
				return DateOnly.Today;
			var period = periodInWorkflowControlSet.Invoke(workflowControlSet);

			return period.StartDate < DateOnly.Today ? 
							DateOnly.Today : 
							period.StartDate;
		}
	}
}