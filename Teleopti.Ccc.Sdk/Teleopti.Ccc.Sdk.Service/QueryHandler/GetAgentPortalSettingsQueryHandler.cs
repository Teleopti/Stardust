﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.WcfService.Factory;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.QueryHandler
{
    public class GetAgentPortalSettingsQueryHandler : IHandleQuery<GetAgentPortalSettingsQueryDto, ICollection<AgentPortalSettingsDto>>
    {
        private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public GetAgentPortalSettingsQueryHandler(IPersonalSettingDataRepository personalSettingDataRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _personalSettingDataRepository = personalSettingDataRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public ICollection<AgentPortalSettingsDto> Handle(GetAgentPortalSettingsQueryDto query)
        {
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var setting = _personalSettingDataRepository.FindValueByKey("AgentPortalSettings", new AgentPortalSettings{Resolution = 4});
                return new List<AgentPortalSettingsDto> {new AgentPortalSettingsDto {Resolution = setting.Resolution}};
                
            }
        }
    }
}
