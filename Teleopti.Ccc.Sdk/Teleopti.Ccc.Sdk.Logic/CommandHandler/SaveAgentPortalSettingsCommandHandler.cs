using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class SaveAgentPortalSettingsCommandHandler : IHandleCommand<SaveAgentPortalSettingsCommandDto>
	{
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public SaveAgentPortalSettingsCommandHandler(IPersonalSettingDataRepository personalSettingDataRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_personalSettingDataRepository = personalSettingDataRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(SaveAgentPortalSettingsCommandDto command)
		{
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var setting = _personalSettingDataRepository.FindValueByKey("AgentPortalSettings", new AgentPortalSettings());
				setting.Resolution = command.Resolution;
				_personalSettingDataRepository.PersistSettingValue(setting);
				uow.PersistAll();
			}
			command.Result = new CommandResultDto { AffectedItems = 1 };
		}
	}
}
