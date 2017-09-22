using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class ShiftCategoryAssembler : Assembler<IShiftCategory,ShiftCategoryDto>
    {
        private readonly IShiftCategoryRepository _repository;

        public ShiftCategoryAssembler(IShiftCategoryRepository repository)
        {
            _repository = repository;
        }

        public override ShiftCategoryDto DomainEntityToDto(IShiftCategory entity)
        {
            return new ShiftCategoryDto
                       {
                           Name = entity.Description.Name,
                           ShortName = entity.Description.ShortName,
                           Id = entity.Id,
                           DisplayColor = new ColorDto(entity.DisplayColor)
                       };
        }

        public override IShiftCategory DtoToDomainEntity(ShiftCategoryDto dto)
        {
            IShiftCategory shiftCategory = _repository.Get(dto.Id.GetValueOrDefault(Guid.Empty));
            return shiftCategory;
        }
    }
}