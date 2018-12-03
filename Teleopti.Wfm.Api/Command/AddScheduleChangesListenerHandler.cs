using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Wfm.Api.Command
{
	public class AddScheduleChangesListenerHandler : ICommandHandler<AddScheduleChangesListenerDto>
	{
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly IPermissionProvider _permissionProvider;

		public AddScheduleChangesListenerHandler(IGlobalSettingDataRepository globalSettingDataRepository, IPermissionProvider permissionProvider)
		{
			_globalSettingDataRepository = globalSettingDataRepository;
			_permissionProvider = permissionProvider;
		}

		[UnitOfWork]
		public virtual ResultDto Handle(AddScheduleChangesListenerDto command)
		{
			if (!_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths
				.WebPermissions))
			{
				return new ResultDto {Successful = false, Message = "This function requires higher permissions."};
			}
			if (command.Name.IsNullOrEmpty() || string.IsNullOrWhiteSpace(command.Name)) return new ResultDto { Successful = false, Message = "Name must not be null or empty"};
			if (command.DaysEndFromCurrentDate < command.DaysStartFromCurrentDate) return new ResultDto { Successful = false, Message = "DaysEndFromCurrentDate must not be less than DaysStartFromCurrentDate." };
			if (!Uri.TryCreate(command.Url, UriKind.Absolute, out var uri)) return new ResultDto { Successful = false, Message = "Invalid Url"};
			
			var setting =
				_globalSettingDataRepository.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions());
			setting.Add(new ScheduleChangeListener
			{
				Name = command.Name,
				RelativeDateRange =
					new MinMax<int>(command.DaysStartFromCurrentDate, command.DaysEndFromCurrentDate),
				Uri = uri
			});
				_globalSettingDataRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, setting);
			return new ResultDto {Successful = true};
		}
	}
}