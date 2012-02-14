using System;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDomainDataMappingProfile : Profile
	{
		private readonly Func<IVirtualSchedulePeriodProvider> _virtualSchedulePeriodProvider;
		private readonly Func<IPreferenceProvider> _preferenceProvider;
		private readonly Func<ILoggedOnUser> _loggedOnUser;
		private readonly Func<IWorkTimeMinMaxCalculator> _workTimeMinMaxCalculator;

		public PreferenceDomainDataMappingProfile(Func<IVirtualSchedulePeriodProvider> virtualSchedulePeriodProvider,
																Func<IPreferenceProvider> preferenceProvider,
																Func<ILoggedOnUser> loggedOnUser,
																Func<IWorkTimeMinMaxCalculator> workTimeMinMaxCalculator)
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
															Period = _virtualSchedulePeriodProvider().GetCurrentOrNextVirtualPeriodForDate(s),
															WorkflowControlSet = _loggedOnUser().CurrentUser().WorkflowControlSet
														};
									domainData.PreferenceDays = _preferenceProvider().GetPreferencesForPeriod(domainData.Period);
									domainData.WorkTimeMinMax = from d in domainData.Period.DayCollection()
									                            let personPeriod = _loggedOnUser().CurrentUser().PersonPeriods(new DateOnlyPeriod(d, d)).
									                            	FirstOrDefault()
									                            where personPeriod != null
									                            let ruleSetBag =
																	personPeriod.RuleSetBag
																where ruleSetBag != null
																select
																	new WorkTimeMinMaxDomainData(_workTimeMinMaxCalculator().WorkTimeMinMax(ruleSetBag, d), d);
									return domainData;
								});
		}
	}
}