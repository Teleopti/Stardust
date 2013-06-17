using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

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
            IScheduleTag scheduleTag = NullScheduleTag.Instance;
			if (dto != null && dto.Id.HasValue)
            {
                scheduleTag = _scheduleTagRepository.Get(dto.Id.GetValueOrDefault());
            }
            return scheduleTag;
        }
    }
}
