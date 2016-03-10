using System;
using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Interfaces.Infrastructure;

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
				var scheduleChangesSubscriptionsDto = new ScheduleChangesSubscriptionsDto();
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
			var principalAuthorization = PrincipalAuthorization.Instance();
			if (!principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage) &&
				!principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebPermissions))
			{
				throw new FaultException("This function requires higher permissions.");
			}
		}
	}
}