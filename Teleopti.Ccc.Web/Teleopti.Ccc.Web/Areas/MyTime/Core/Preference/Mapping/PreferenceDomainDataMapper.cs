using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDomainDataMapper
	{
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly IMustHaveRestrictionProvider _mustHaveRestrictionProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public PreferenceDomainDataMapper(IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider, ILoggedOnUser loggedOnUser, IMustHaveRestrictionProvider mustHaveRestrictionProvider)
		{
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_loggedOnUser = loggedOnUser;
			_mustHaveRestrictionProvider = mustHaveRestrictionProvider;
		}

		public PreferenceDomainData Map(DateOnly s)
		{
			var period = _virtualSchedulePeriodProvider.GetCurrentOrNextVirtualPeriodForDate(s);
			var dates = period.DayCollection();
			var days = dates.Select(d => new PreferenceDayDomainData { Date = d }).ToArray();
			var currentUser = _loggedOnUser.CurrentUser();
			var workflowControlSet = currentUser.WorkflowControlSet;
			var virtualSchedulePeriod = _virtualSchedulePeriodProvider.VirtualSchedulePeriodForDate(s);
			var maxMustHave = virtualSchedulePeriod?.MustHavePreference ?? 0;
			var currentMustHave = _mustHaveRestrictionProvider.CountMustHave(s, currentUser);

			return new PreferenceDomainData
			{
				SelectedDate = s,
				Period = period,
				WorkflowControlSet = workflowControlSet,
				Days = days,
				MaxMustHave = maxMustHave,
				CurrentMustHave = currentMustHave
			};
		}
	}
}