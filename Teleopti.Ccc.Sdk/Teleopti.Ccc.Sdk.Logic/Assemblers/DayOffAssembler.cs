using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class DayOffAssembler : Assembler<IDayOffTemplate,DayOffInfoDto>
    {
        private readonly IDayOffTemplateRepository _dayOffRepository;

        public DayOffAssembler(IDayOffTemplateRepository dayOffRepository)
        {
            _dayOffRepository = dayOffRepository;
        }

        public override DayOffInfoDto DomainEntityToDto(IDayOffTemplate entity)
        {
            return new DayOffInfoDto
                       {
                           Id = entity.Id,
                           Name = entity.Description.Name,
                           ShortName = entity.Description.ShortName,
                           IsDeleted = ((IDeleteTag) entity).IsDeleted,
						   PayrollCode = entity.PayrollCode
                       };
        }

        public override IDayOffTemplate DtoToDomainEntity(DayOffInfoDto dto)
        {
            return _dayOffRepository.Get(dto.Id.GetValueOrDefault(Guid.Empty));
        }
    }
}