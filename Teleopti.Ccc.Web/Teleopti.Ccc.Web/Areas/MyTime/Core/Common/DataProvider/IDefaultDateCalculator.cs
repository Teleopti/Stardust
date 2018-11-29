using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IDefaultDateCalculator
	{
		DateOnly Calculate (IWorkflowControlSet workflowControlSet,
			Func<IWorkflowControlSet, DateOnlyPeriod> periodInWorkflowControlSet, IList<IPersonPeriod> personPeriods);
	}
}