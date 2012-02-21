using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDomainDataMappingProfile : Profile
	{
		private readonly IResolve<IVirtualSchedulePeriodProvider> _virtualSchedulePeriodProvider;
		private readonly IResolve<IPreferenceProvider> _preferenceProvider;
		private readonly IResolve<ILoggedOnUser> _loggedOnUser;
		private readonly IResolve<IPreferenceFeedbackProvider> _preferenceFeedbackProvider;

		public PreferenceDomainDataMappingProfile(
			IResolve<IVirtualSchedulePeriodProvider> virtualSchedulePeriodProvider,
			IResolve<IPreferenceProvider> preferenceProvider,
			IResolve<ILoggedOnUser> loggedOnUser,
			IResolve<IPreferenceFeedbackProvider> preferenceFeedbackProvider
			)
		{
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_preferenceProvider = preferenceProvider;
			_loggedOnUser = loggedOnUser;
			_preferenceFeedbackProvider = preferenceFeedbackProvider;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<DateOnly, PreferenceDomainData>()
				.ConvertUsing(s =>
				              	{
				              		var period = _virtualSchedulePeriodProvider.Invoke().GetCurrentOrNextVirtualPeriodForDate(s);
				              		var preferenceDays = _preferenceProvider.Invoke().GetPreferencesForPeriod(period);
				              		preferenceDays = preferenceDays ?? new IPreferenceDay[] {};
									var dates = period.DayCollection();
				              		var days = (from d in dates
				              		            let preferenceDay = (from pd in preferenceDays where pd.RestrictionDate == d select pd).SingleOrDefault()
												let workTimeMinMax = _preferenceFeedbackProvider.Invoke().WorkTimeMinMaxForDate(d)
				              		            select new PreferenceDayDomainData
				              		                   	{
				              		                   		Date = d,
				              		                   		PreferenceDay = preferenceDay,
															WorkTimeMinMax = workTimeMinMax
				              		                   	}).ToArray();
									
				              		return new PreferenceDomainData
				              		       	{
				              		       		SelectedDate = s,
				              		       		Period = period,
				              		       		WorkflowControlSet = _loggedOnUser.Invoke().CurrentUser().WorkflowControlSet,
				              		       		Days = days
				              		       	};
				              	});
		}
	}
}