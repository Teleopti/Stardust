using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.WcfService.Factory;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.CommandHandler
{
    public class SaveAgentPortalSettingsCommandHandler : IHandleCommand<SaveAgentPortalSettingsCommandDto>
    {
        private  IPersonalSettingDataRepository _personalSettingDataRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;


        public SaveAgentPortalSettingsCommandHandler(IPersonalSettingDataRepository personalSettingDataRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _personalSettingDataRepository = personalSettingDataRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public CommandResultDto Handle(SaveAgentPortalSettingsCommandDto command)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                _personalSettingDataRepository = new PersonalSettingDataRepository(uow);
                //for now we just save the resolution
                var setting = _personalSettingDataRepository.FindValueByKey("AgentPortalSettings", new AgentPortalSettings());
                setting.Resolution = command.Resolution;
                _personalSettingDataRepository.PersistSettingValue(setting);
                uow.PersistAll();
            }
            return new CommandResultDto { AffectedItems = 1 };
        }
    }
}
