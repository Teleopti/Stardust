using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class AbsenceAssembler:Assembler<IAbsence, AbsenceDto>
    {
        private readonly IAbsenceRepository _absenceRepository;

        public AbsenceAssembler(IAbsenceRepository absenceRepository)
        {
            _absenceRepository = absenceRepository;
        }

        public override AbsenceDto DomainEntityToDto(IAbsence entity)
        {
            AbsenceDto absenceDto = new AbsenceDto
                                        {
                                            Id = entity.Id,
                                            Name = entity.Description.Name,
                                            ShortName = entity.Description.ShortName,
                                            Priority = entity.Priority,
                                            InContractTime = entity.InContractTime,
                                            DisplayColor = new ColorDto(entity.DisplayColor),
                                            PayrollCode = entity.PayrollCode,
                                            IsTrackable = entity.Tracker != null,
                                            IsDeleted = ((IDeleteTag) entity).IsDeleted,
                                            InPaidTime = entity.InPaidTime,
                                            InWorkTime = entity.InWorkTime
                                        };
            return absenceDto;
        }

        public override IAbsence DtoToDomainEntity(AbsenceDto dto)
        {
            IAbsence absence = null;
            if (dto.Id.HasValue)
            {
                absence = _absenceRepository.Get(dto.Id.Value);
            }
            return absence;
        }
    }
}