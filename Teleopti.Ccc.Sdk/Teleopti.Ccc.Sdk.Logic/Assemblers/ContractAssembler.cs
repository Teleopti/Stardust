using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
                                   EmploymentType = entity.EmploymentType,
                                   Id = entity.Id.GetValueOrDefault(Guid.Empty),
                                   IsDeleted = ((IDeleteTag)entity).IsDeleted
                               };
            foreach (IMultiplicatorDefinitionSet multiplicatorDefinitionSet in entity.MultiplicatorDefinitionSetCollection)
            {
                if (multiplicatorDefinitionSet.MultiplicatorType != MultiplicatorType.OBTime)
                {
                    contract.AvailableOvertimeDefinitionSets.Add(multiplicatorDefinitionSet.Id.GetValueOrDefault(Guid.Empty));
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