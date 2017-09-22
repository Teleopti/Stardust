using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetAgentPortalSettingsQueryHandler : IHandleQuery<GetAgentPortalSettingsQueryDto, ICollection<AgentPortalSettingsDto>>
    {
        private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

        public GetAgentPortalSettingsQueryHandler(IPersonalSettingDataRepository personalSettingDataRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
        {
            _personalSettingDataRepository = personalSettingDataRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public ICollection<AgentPortalSettingsDto> Handle(GetAgentPortalSettingsQueryDto query)
        {
            using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var setting = _personalSettingDataRepository.FindValueByKey("AgentPortalSettings", new AgentPortalSettings{Resolution = 4});
                return new List<AgentPortalSettingsDto> {new AgentPortalSettingsDto {Resolution = setting.Resolution}};
                
            }
        }
    }
}
