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
		private readonly IResolve<ILoggedOnUser> _loggedOnUser;
		private readonly IResolve<IScheduleProvider> _scheduleProvider;
		private readonly IResolve<IProjectionProvider> _projectionProvider;

		public PreferenceDomainDataMappingProfile(IResolve<IVirtualSchedulePeriodProvider> virtualSchedulePeriodProvider, IResolve<ILoggedOnUser> loggedOnUser, IResolve<IScheduleProvider> scheduleProvider, IResolve<IProjectionProvider> projectionProvider)
		{
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_loggedOnUser = loggedOnUser;
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
									var scheduleDays = _scheduleProvider.Invoke().GetScheduleForPeriod(period) ?? new IScheduleDay[] { };
									var dates = period.DayCollection();
				              		var days = (from d in dates
												let providedScheduleDay = (from sd in scheduleDays where sd.DateOnlyAsPeriod.DateOnly == d select sd).SingleOrDefault()
												let scheduleDay = providedScheduleDay != null && providedScheduleDay.IsScheduled() ? providedScheduleDay : null
												let projection = scheduleDay != null ? _projectionProvider.Invoke().Projection(scheduleDay) : null
				              		            select new PreferenceDayDomainData
				              		                   	{
				              		                   		Date = d,
															ScheduleDay = scheduleDay,
															Projection = projection
				              		                   	}).ToArray();
				              		var workflowControlSet = _loggedOnUser.Invoke().CurrentUser().WorkflowControlSet;
									var colorSource = new ScheduleColorSource
														{
															ScheduleDays = scheduleDays,
															Projections = (from p in days where p.Projection != null select p.Projection).ToArray(),
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