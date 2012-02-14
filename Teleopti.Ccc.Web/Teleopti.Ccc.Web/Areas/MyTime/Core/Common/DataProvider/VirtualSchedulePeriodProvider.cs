﻿using System;
using System.Linq;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class VirtualSchedulePeriodProvider : IVirtualSchedulePeriodProvider
	{
		private readonly ILoggedOnUser _personProvider;
		private readonly IDefaultDateCalculator _defaultDateCalculator;

		public VirtualSchedulePeriodProvider(ILoggedOnUser personProvider, IDefaultDateCalculator defaultDateCalculator)
		{
			_personProvider = personProvider;
			_defaultDateCalculator = defaultDateCalculator;
		}

		public DateOnlyPeriod GetCurrentOrNextVirtualPeriodForDate(DateOnly date)
		{
			var person = _personProvider.CurrentUser();
			var virtualSchedulePeriod = person.VirtualSchedulePeriodOrNext(date);
			return virtualSchedulePeriod.DateOnlyPeriod;
		}

		public bool HasSchedulePeriod()
		{
			var person = _personProvider.CurrentUser();
			return person.PersonSchedulePeriodCollection.Any();
		}

		public static readonly Func<IWorkflowControlSet, DateOnlyPeriod> StudentAvailabilityPeriod = w => w.StudentAvailabilityPeriod;
		public static readonly Func<IWorkflowControlSet, DateOnlyPeriod> PreferencePeriod = w => w.PreferencePeriod;

		public DateOnly CalculateStudentAvailabilityDefaultDate()
		{
			var person = _personProvider.CurrentUser();
			return _defaultDateCalculator.Calculate(person.WorkflowControlSet, StudentAvailabilityPeriod);
		}

		public DateOnly CalculatePreferenceDefaultDate()
		{
			var person = _personProvider.CurrentUser();
			return _defaultDateCalculator.Calculate(person.WorkflowControlSet, PreferencePeriod);
		}
	}
}