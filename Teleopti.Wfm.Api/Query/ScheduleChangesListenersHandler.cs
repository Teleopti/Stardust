using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Wfm.Api.Query.Request;
using Teleopti.Wfm.Api.Query.Response;

namespace Teleopti.Wfm.Api.Query
{
	public class ScheduleChangesListenersHandler : IQueryHandler<AllScheduleChangesListenerSubscriptionDto,
		ScheduleChangesListenerSubscriptionDto>
	{
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		public ScheduleChangesListenersHandler(IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		[UnitOfWork]
		public virtual QueryResultDto<ScheduleChangesListenerSubscriptionDto> Handle(AllScheduleChangesListenerSubscriptionDto query)
		{
			if (!PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.WebPermissions))
				return new QueryResultDto<ScheduleChangesListenerSubscriptionDto>
				{
					Successful = false,
					Message = "This function requires higher permissions."
				};

			var setting = _globalSettingDataRepository.FindValueByKey(ScheduleChangeSubscriptions.Key,
				new ScheduleChangeSubscriptions());

			var scheduleChangesSubscriptionsDto = new ScheduleChangesListenerSubscriptionDto
			{
				Exponent = "AQAB",
				Modulus =
					"tcQWMgdpQeCd8+gzB3rYQAehHXF5mBGdyFMkJMEmcQmTlkpg22xLNz/kNYXZ7j2Cuhls+PBORzZkfBsNoL1vErT+N9Es4EEWOt6ntNe7wujqQqktUT/QOWEMJ8zJQM3bn7Oj9H5StBr7DWSRzgEjOc7knDcb4KCQL3ceXqmqwSonPfP1hp+bE8rZuxDISYiZVEkm417YzUHBk3ppV30Q9zvfL9IZX0q/ebCTRnLFockl7yOVucomvo8j4ssFPCAYgASoNvzWq+s5UTzYELl1I7F3hQnFwx0bIpQFmGbZ5BbNczc6rVYtCX5KDMsVaJSUcXBAnqGd20hq/ICkBR658w=="
			};
			Array.ForEach(setting.Subscriptions(),
				s =>
					scheduleChangesSubscriptionsDto.Listeners.Add(new ScheduleChangesListenerDto
					{
						Name = s.Name,
						Url = s.Uri.ToString(),
						DaysStartFromCurrentDate = s.RelativeDateRange.Minimum,
						DaysEndFromCurrentDate = s.RelativeDateRange.Maximum
					}));

			return new QueryResultDto<ScheduleChangesListenerSubscriptionDto>
			{
				Successful = true,
				Result = new[] {scheduleChangesSubscriptionsDto}
			};
		}
	}
}