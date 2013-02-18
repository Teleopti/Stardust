using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class ContractRepository
    {
    	private readonly ITeleoptiOrganizationService _teleoptiOrganizationService;
    	private Dictionary<Guid, ContractDto> _contractDictionary;

    	public ContractRepository(ITeleoptiOrganizationService teleoptiOrganizationService)
    	{
    		_teleoptiOrganizationService = teleoptiOrganizationService;
    	}

    	public void Initialize()
        {
            var contracts = _teleoptiOrganizationService.GetContracts(new LoadOptionDto {LoadDeleted = true});
            _contractDictionary = contracts.ToDictionary(k => k.Id.GetValueOrDefault(), v => v);
        }

        public ContractDto GetById(Guid id)
        {
            ContractDto contractDto;
            if (!_contractDictionary.TryGetValue(id,out contractDto))
            {
                contractDto = new ContractDto();
            }
            return contractDto;
        }
    }
}
