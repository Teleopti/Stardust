using System.Linq;
using AutoMapper;
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
		private readonly IResolve<IWorkTimeMinMaxCalculator> _workTimeMinMaxCalculator;

		public PreferenceDomainDataMappingProfile(
			IResolve<IVirtualSchedulePeriodProvider> virtualSchedulePeriodProvider,
			IResolve<IPreferenceProvider> preferenceProvider,
			IResolve<ILoggedOnUser> loggedOnUser,
			IResolve<IWorkTimeMinMaxCalculator> workTimeMinMaxCalculator
			)
		{
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_preferenceProvider = preferenceProvider;
			_loggedOnUser = loggedOnUser;
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<DateOnly, PreferenceDomainData>()
				.ConvertUsing(s =>
				              	{
				              		var domainData = new PreferenceDomainData
				              		                 	{
				              		                 		SelectedDate = s,
				              		                 		Period = _virtualSchedulePeriodProvider.Invoke().GetCurrentOrNextVirtualPeriodForDate(s),
				              		                 		WorkflowControlSet = _loggedOnUser.Invoke().CurrentUser().WorkflowControlSet
				              		                 	};
				              		domainData.PreferenceDays = _preferenceProvider.Invoke().GetPreferencesForPeriod(domainData.Period);
				              		domainData.WorkTimeMinMax = from d in domainData.Period.DayCollection()
				              		                            let personPeriod = _loggedOnUser.Invoke().CurrentUser().PersonPeriods(new DateOnlyPeriod(d, d)).
				              		                            	FirstOrDefault()
				              		                            where personPeriod != null
				              		                            let ruleSetBag =
				              		                            	personPeriod.RuleSetBag
				              		                            where ruleSetBag != null
				              		                            select
				              		                            	new WorkTimeMinMaxDomainData(_workTimeMinMaxCalculator.Invoke().WorkTimeMinMax(ruleSetBag, d), d);
				              		return domainData;
				              	});
		}
	}
}