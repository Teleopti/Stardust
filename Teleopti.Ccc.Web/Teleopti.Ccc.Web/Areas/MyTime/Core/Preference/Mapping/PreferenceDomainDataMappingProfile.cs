using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDomainDataMappingProfile : Profile
	{
		private readonly IResolve<IVirtualSchedulePeriodProvider> _virtualSchedulePeriodProvider;
		private readonly IResolve<ILoggedOnUser> _loggedOnUser;

		public PreferenceDomainDataMappingProfile(IResolve<IVirtualSchedulePeriodProvider> virtualSchedulePeriodProvider, IResolve<ILoggedOnUser> loggedOnUser)
		{
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_loggedOnUser = loggedOnUser;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<DateOnly, PreferenceDomainData>()
				.ConvertUsing(s =>
				              	{
				              		var period = _virtualSchedulePeriodProvider.Invoke().GetCurrentOrNextVirtualPeriodForDate(s);
									var dates = period.DayCollection();
				              		var days = (from d in dates
				              		            select new PreferenceDayDomainData
				              		                   	{
				              		                   		Date = d,
				              		                   	}).ToArray();
				              		var workflowControlSet = _loggedOnUser.Invoke().CurrentUser().WorkflowControlSet;
				              		var virtualSchedulePeriod = _virtualSchedulePeriodProvider.Invoke().VirtualSchedulePeriodForDate(s);
									var maxMustHave = virtualSchedulePeriod == null ? 0 : virtualSchedulePeriod.MustHavePreference;
				              		
				              		return new PreferenceDomainData
				              		       	{
				              		       		SelectedDate = s,
				              		       		Period = period,
				              		       		WorkflowControlSet = workflowControlSet,
				              		       		Days = days,
												MaxMustHave = maxMustHave
				              		       	};
				              	});
		}
	}
}