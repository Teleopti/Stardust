using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class AddScheduleChangesListenerCommandHandler : IHandleCommand<AddScheduleChangesListenerCommandDto>
	{
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public AddScheduleChangesListenerCommandHandler(IGlobalSettingDataRepository globalSettingDataRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_globalSettingDataRepository = globalSettingDataRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void Handle(AddScheduleChangesListenerCommandDto command)
		{
			Uri uri;
			validateListener(command.Listener, out uri);
			validatePermissions();
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var setting = _globalSettingDataRepository.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions());
				setting.Add(new ScheduleChangeListener
				{
					Name = command.Listener.Name,
					RelativeDateRange =
						new MinMax<int>(command.Listener.DaysStartFromCurrentDate, command.Listener.DaysEndFromCurrentDate),
					Uri = uri
				});
				_globalSettingDataRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, setting);
				uow.PersistAll();
			}
			command.Result = new CommandResultDto { AffectedId = null, AffectedItems = 1 };
		}

		private void validateListener(ScheduleChangesListenerDto listener, out Uri uri)
		{
			if (listener == null || string.IsNullOrWhiteSpace(listener.Name))
			{
				throw new FaultException("The listener must have a name defined.");
			}
			if (listener.DaysEndFromCurrentDate < listener.DaysStartFromCurrentDate)
			{
				throw new FaultException("The listener relative start date cannot be set after the end date.");
			}
			if (!Uri.TryCreate(listener.Url, UriKind.Absolute, out uri))
			{
				throw new FaultException("The given url cannot be parsed as a valid uri.");
			}
		}

		private static void validatePermissions()
		{
			var principalAuthorization = PrincipalAuthorization.Current();
			if (!principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebPermissions))
			{
				throw new FaultException("This function requires higher permissions.");
			}
		}
	}
}