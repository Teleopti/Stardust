using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public class ContractAssembler : Assembler<IContract,ContractDto>
    {
        private readonly IContractRepository _contractRepository;

        public ContractAssembler(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        public override ContractDto DomainEntityToDto(IContract entity)
        {
            var contract = new ContractDto
                               {
                                   Description = entity.Description.ToString(),
								   EmploymentType = entity.EmploymentType.Convert(),
                                   Id = entity.Id.GetValueOrDefault(Guid.Empty),
                                   IsDeleted = ((IDeleteTag)entity).IsDeleted
                               };

            foreach (IMultiplicatorDefinitionSet multiplicatorDefinitionSet in entity.MultiplicatorDefinitionSetCollection)
            {
                if (multiplicatorDefinitionSet.MultiplicatorType != MultiplicatorType.OBTime)
                {
                    contract.AvailableOvertimeDefinitionSets.Add(multiplicatorDefinitionSet.Id.GetValueOrDefault(Guid.Empty));
                }
                else
                {
					contract.AvailableShiftAllowanceDefinitionSets.Add(multiplicatorDefinitionSet.Id.GetValueOrDefault(Guid.Empty));
                }
            }

            return contract;
        }

        public override IContract DtoToDomainEntity(ContractDto dto)
        {
            return _contractRepository.Get(dto.Id.GetValueOrDefault(Guid.Empty));
        }
    }
}