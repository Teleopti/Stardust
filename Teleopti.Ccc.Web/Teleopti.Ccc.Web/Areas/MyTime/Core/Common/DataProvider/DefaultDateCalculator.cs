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
			var returnDate = workflowControlSetPeriod.StartDate < _now.LocalDateOnly()
									? _now.LocalDateOnly()
									: workflowControlSetPeriod.StartDate;

			//is there a person period active on the workflowcontrolsetperiod (preference period or availability period)
			var activePersonPeriod = personPeriods.FirstOrDefault(pp => 
				pp.Period.Contains (workflowControlSetPeriod.StartDate) &&
				pp.StartDate >= _now.LocalDateOnly());

			//if we have found an active person period, then use the workflowcontrolsetperiod start date.
			if (activePersonPeriod != null)
			{
				return workflowControlSetPeriod.StartDate;
			}

			// if we find a future person period then use the future person period start date
			var futurePersonPeriod = personPeriods.FirstOrDefault(pp => 
				pp.StartDate > workflowControlSetPeriod.StartDate && pp.StartDate >= _now.LocalDateOnly());

			
			// otherwise use return date.
			return futurePersonPeriod == null ? returnDate : futurePersonPeriod.StartDate;
			 
		}
	}
}