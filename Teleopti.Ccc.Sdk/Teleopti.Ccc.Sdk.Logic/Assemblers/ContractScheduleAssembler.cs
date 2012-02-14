using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class ContractScheduleAssembler : Assembler<IContractSchedule,ContractScheduleDto>
    {
        private readonly IContractScheduleRepository _contractScheduleRepository;

        public ContractScheduleAssembler(IContractScheduleRepository contractScheduleRepository)
        {
            _contractScheduleRepository = contractScheduleRepository;
        }

        public override ContractScheduleDto DomainEntityToDto(IContractSchedule entity)
        {
            return new ContractScheduleDto
                       {
                           Description = entity.Description.Name,
                           Id = entity.Id,
                           IsDeleted = ((IDeleteTag) entity).IsDeleted
                       };
        }

        public override IContractSchedule DtoToDomainEntity(ContractScheduleDto dto)
        {
            return _contractScheduleRepository.Get(dto.Id.GetValueOrDefault(Guid.Empty));
        }
    }
}