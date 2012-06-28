using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
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
		private readonly IResolve<IScheduleProvider> _scheduleProvider;
		private readonly IResolve<IProjectionProvider> _projectionProvider;

		public PreferenceDomainDataMappingProfile(IResolve<IVirtualSchedulePeriodProvider> virtualSchedulePeriodProvider, IResolve<IPreferenceProvider> preferenceProvider, IResolve<ILoggedOnUser> loggedOnUser, IResolve<IPreferenceFeedbackProvider> preferenceFeedbackProvider, IResolve<IScheduleProvider> scheduleProvider, IResolve<IProjectionProvider> projectionProvider)
		{
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_preferenceProvider = preferenceProvider;
			_loggedOnUser = loggedOnUser;
			_preferenceFeedbackProvider = preferenceFeedbackProvider;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<DateOnly, PreferenceDomainData>()
				.ConvertUsing(s =>
				              	{
				              		var period = _virtualSchedulePeriodProvider.Invoke().GetCurrentOrNextVirtualPeriodForDate(s);
				              		var preferenceDays = _preferenceProvider.Invoke().GetPreferencesForPeriod(period);
									var scheduleDays = _scheduleProvider.Invoke().GetScheduleForPeriod(period) ?? new IScheduleDay[] { };
				              		preferenceDays = preferenceDays ?? new IPreferenceDay[] {};
									var dates = period.DayCollection();
				              		var days = (from d in dates
				              		            let preferenceDay = (from pd in preferenceDays where pd.RestrictionDate == d select pd).OrderByDescending(k=>k.UpdatedOn).FirstOrDefault()
												let providedScheduleDay = (from sd in scheduleDays where sd.DateOnlyAsPeriod.DateOnly == d select sd).SingleOrDefault()
												let scheduleDay = providedScheduleDay != null && providedScheduleDay.IsScheduled() ? providedScheduleDay : null
												let projection = scheduleDay != null ? _projectionProvider.Invoke().Projection(scheduleDay) : null
												//let workTimeMinMax = _preferenceFeedbackProvider.Invoke().WorkTimeMinMaxForDate(d, providedScheduleDay)
				              		            select new PreferenceDayDomainData
				              		                   	{
				              		                   		Date = d,
				              		                   		PreferenceDay = preferenceDay,
															//WorkTimeMinMax = workTimeMinMax,
															ScheduleDay = scheduleDay,
															Projection = projection
				              		                   	}).ToArray();
				              		var workflowControlSet = _loggedOnUser.Invoke().CurrentUser().WorkflowControlSet;
									var colorSource = new ScheduleColorSource
														{
															ScheduleDays = scheduleDays,
															Projections = (from p in days where p.Projection != null select p.Projection).ToArray(),
															PreferenceDays = preferenceDays,
															WorkflowControlSet = workflowControlSet
														};
				              		return new PreferenceDomainData
				              		       	{
				              		       		SelectedDate = s,
				              		       		Period = period,
				              		       		WorkflowControlSet = workflowControlSet,
				              		       		Days = days,
												ColorSource = colorSource
				              		       	};
				              	});
		}
	}
}