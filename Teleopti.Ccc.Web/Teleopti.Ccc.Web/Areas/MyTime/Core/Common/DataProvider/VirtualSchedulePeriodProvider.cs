using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


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

		public IVirtualSchedulePeriod VirtualSchedulePeriodForDate(DateOnly date)
		{
			var person = _personProvider.CurrentUser();
			var virtualSchedulePeriod = person.VirtualSchedulePeriodOrNext(date);
			return virtualSchedulePeriod;
		}

		public DateOnlyPeriod GetCurrentOrNextVirtualPeriodForDate(DateOnly date)
		{
			return VirtualSchedulePeriodForDate(date).DateOnlyPeriod;
		}

		public bool MissingSchedulePeriod()
		{
			var person = _personProvider.CurrentUser();
			return !person.PersonSchedulePeriodCollection.Any();
		}

		public bool MissingPersonPeriod(DateOnly? date)
		{
			var person = _personProvider.CurrentUser();
			var periods = person.PersonPeriodCollection;
			return periods == null || periods.All(personPeriod => date == null || !personPeriod.Period.Contains(date.Value));
		}

		public static readonly Func<IWorkflowControlSet, DateOnlyPeriod> StudentAvailabilityPeriod = w => w.StudentAvailabilityPeriod;
		public static readonly Func<IWorkflowControlSet, DateOnlyPeriod> PreferencePeriod = w => w.PreferencePeriod;

		public DateOnly CalculateStudentAvailabilityDefaultDate()
		{
			var person = _personProvider.CurrentUser();
			return _defaultDateCalculator.Calculate(person.WorkflowControlSet, StudentAvailabilityPeriod, person.PersonPeriodCollection);
		}

		public DateOnly CalculatePreferenceDefaultDate()
		{
			var person = _personProvider.CurrentUser();
			return _defaultDateCalculator.Calculate(person.WorkflowControlSet, PreferencePeriod, person.PersonPeriodCollection);
		}
	}
}