﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IDefaultDateCalculator
	{
		DateOnly Calculate(IWorkflowControlSet workflowControlSet, Func<IWorkflowControlSet, DateOnlyPeriod> periodInWorkflowControlSet);
	}
}