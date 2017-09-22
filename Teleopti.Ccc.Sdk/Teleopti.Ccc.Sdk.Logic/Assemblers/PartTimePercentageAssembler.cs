using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PartTimePercentageAssembler : Assembler<IPartTimePercentage,PartTimePercentageDto>
    {
        private readonly IPartTimePercentageRepository _partTimePercentageRepository;

        public PartTimePercentageAssembler(IPartTimePercentageRepository partTimePercentageRepository)
        {
            _partTimePercentageRepository = partTimePercentageRepository;
        }

        public override PartTimePercentageDto DomainEntityToDto(IPartTimePercentage entity)
        {
            return new PartTimePercentageDto
                       {
                           Description = entity.Description.ToString(),
                           Percentage = entity.Percentage.Value,
                           Id = entity.Id.GetValueOrDefault(Guid.Empty),
                           IsDeleted = ((IDeleteTag) entity).IsDeleted
                       };
        }

        public override IPartTimePercentage DtoToDomainEntity(PartTimePercentageDto dto)
        {
            return _partTimePercentageRepository.Get(dto.Id.GetValueOrDefault(Guid.Empty));
        }
    }
}