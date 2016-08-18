using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDomainDataMappingProfile : Profile
	{
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly IMustHaveRestrictionProvider _mustHaveRestrictionProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public PreferenceDomainDataMappingProfile(IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider, ILoggedOnUser loggedOnUser, IMustHaveRestrictionProvider mustHaveRestrictionProvider)
		{
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_loggedOnUser = loggedOnUser;
			_mustHaveRestrictionProvider = mustHaveRestrictionProvider;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<DateOnly, PreferenceDomainData>()
				.ConvertUsing(s =>
				              	{
				              		var period = _virtualSchedulePeriodProvider.GetCurrentOrNextVirtualPeriodForDate(s);
									var dates = period.DayCollection();
				              		var days = (from d in dates
				              		            select new PreferenceDayDomainData
				              		                   	{
				              		                   		Date = d,
				              		                   	}).ToArray();
				              		var workflowControlSet = _loggedOnUser.CurrentUser().WorkflowControlSet;
				              		var virtualSchedulePeriod = _virtualSchedulePeriodProvider.VirtualSchedulePeriodForDate(s);
									var maxMustHave = virtualSchedulePeriod == null ? 0 : virtualSchedulePeriod.MustHavePreference;
				              		var currentMustHave = _mustHaveRestrictionProvider.CountMustHave(s, _loggedOnUser.CurrentUser());


				              		return new PreferenceDomainData
				              		       	{
				              		       		SelectedDate = s,
				              		       		Period = period,
				              		       		WorkflowControlSet = workflowControlSet,
				              		       		Days = days,
												MaxMustHave = maxMustHave,
												CurrentMustHave = currentMustHave
									  };
				              	});
		}
	}
}