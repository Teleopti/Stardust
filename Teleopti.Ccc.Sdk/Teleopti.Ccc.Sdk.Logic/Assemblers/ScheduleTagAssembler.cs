using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public interface IScheduleTagAssembler :IAssembler<IScheduleTag, ScheduleTagDto>
	{}

	public class ScheduleTagAssembler : Assembler<IScheduleTag, ScheduleTagDto>, IScheduleTagAssembler
    {
        private readonly IScheduleTagRepository _scheduleTagRepository;

        public ScheduleTagAssembler(IScheduleTagRepository scheduleTagRepository)
        {
            _scheduleTagRepository = scheduleTagRepository;
        }

        public override ScheduleTagDto DomainEntityToDto(IScheduleTag entity)
        {
            var scheduleTagDto = new ScheduleTagDto
            {
                Id = entity.Id,
                Description = entity.Description,
            };
            return scheduleTagDto;
        }

        public override IScheduleTag DtoToDomainEntity(ScheduleTagDto dto)
        {
            IScheduleTag scheduleTag = KeepOriginalScheduleTag.Instance;
			if (dto?.Id != null)
            {
                scheduleTag = _scheduleTagRepository.Get(dto.Id.GetValueOrDefault());
            }
            return scheduleTag;
        }
    }
}
