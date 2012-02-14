using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class ContractScheduleRepository
    {
        private Dictionary<Guid, ContractScheduleDto> _contractScheduleDictionary;

        public void Initialize(ITeleoptiOrganizationService teleoptiOrganizationService)
        {
            var contractSchedules = teleoptiOrganizationService.GetContractSchedules(new LoadOptionDto { LoadDeleted = true });
            _contractScheduleDictionary = contractSchedules.ToDictionary(k => k.Id.GetValueOrDefault(), v => v);
        }

        public ContractScheduleDto GetById(Guid id)
        {
            ContractScheduleDto contractScheduleDto;
            if (!_contractScheduleDictionary.TryGetValue(id, out contractScheduleDto))
            {
                contractScheduleDto = new ContractScheduleDto();
            }
            return contractScheduleDto;
        }
    }
}