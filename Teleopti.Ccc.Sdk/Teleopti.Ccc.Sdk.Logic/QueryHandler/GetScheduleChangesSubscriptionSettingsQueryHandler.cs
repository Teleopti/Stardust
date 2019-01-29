using System;
using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetScheduleChangesSubscriptionSettingsQueryHandler : IHandleQuery<GetScheduleChangesSubscriptionSettingsQueryDto, ICollection<ScheduleChangesSubscriptionsDto>>
	{
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetScheduleChangesSubscriptionSettingsQueryHandler(IGlobalSettingDataRepository globalSettingDataRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_globalSettingDataRepository = globalSettingDataRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<ScheduleChangesSubscriptionsDto> Handle(GetScheduleChangesSubscriptionSettingsQueryDto query)
		{
			validatePermissions();
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var setting = _globalSettingDataRepository.FindValueByKey(ScheduleChangeSubscriptions.Key,
					new ScheduleChangeSubscriptions());
				var scheduleChangesSubscriptionsDto = new ScheduleChangesSubscriptionsDto
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
				return new[] {scheduleChangesSubscriptionsDto};
			}
		}

		private static void validatePermissions()
		{
			var principalAuthorization = PrincipalAuthorization.Current_DONTUSE();
			if (!principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebPermissions))
			{
				throw new FaultException("This function requires higher permissions.");
			}
		}
	}
}