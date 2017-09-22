using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class ActivityAssembler:Assembler<IActivity, ActivityDto>
    {
        private readonly IActivityRepository _activityRepository;

        public ActivityAssembler(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository;
        }

        public override ActivityDto DomainEntityToDto(IActivity entity)
        {
            ActivityDto absenceDto = new ActivityDto
                                        {
                                            Id = entity.Id,
                                            Description = entity.Description.Name,
                                            InContractTime = entity.InContractTime,
                                            DisplayColor = new ColorDto(entity.DisplayColor),
                                            PayrollCode = entity.PayrollCode,
                                            IsDeleted = entity.IsDeleted,
                                            InPaidTime = entity.InPaidTime,
                                            InWorkTime = entity.InWorkTime
                                        };
            return absenceDto;
        }

        public override IActivity DtoToDomainEntity(ActivityDto dto)
        {
            IActivity activity = null;
            if (dto.Id.HasValue)
            {
                activity = _activityRepository.Get(dto.Id.Value);
            }
            return activity;
        }
    }
}