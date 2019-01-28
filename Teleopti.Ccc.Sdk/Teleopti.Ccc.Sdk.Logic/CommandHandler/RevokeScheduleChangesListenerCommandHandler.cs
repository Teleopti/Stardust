using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class RevokeScheduleChangesListenerCommandHandler : IHandleCommand<RevokeScheduleChangesListenerCommandDto>
	{
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public RevokeScheduleChangesListenerCommandHandler(IGlobalSettingDataRepository globalSettingDataRepository,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_globalSettingDataRepository = globalSettingDataRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void Handle(RevokeScheduleChangesListenerCommandDto command)
		{
			validatePermissions();
			var affectedItems = 0;
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var setting = _globalSettingDataRepository.FindValueByKey(ScheduleChangeSubscriptions.Key,
					new ScheduleChangeSubscriptions());
				var foundSetting = setting.Subscriptions().FirstOrDefault(s => s.Name == command.ListenerName);
				if (foundSetting != null)
				{
					affectedItems = 1;
					setting.Remove(foundSetting);
					_globalSettingDataRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, setting);
					uow.PersistAll();
				}

			}
			command.Result = new CommandResultDto {AffectedItems = affectedItems};
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