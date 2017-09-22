using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class SkillAssembler:Assembler<ISkill, SkillDto>
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IAssembler<IActivity, ActivityDto> _activityAssembler;

        public SkillAssembler(ISkillRepository skillRepository, IAssembler<IActivity,ActivityDto> activityAssembler)
        {
            _skillRepository = skillRepository;
            _activityAssembler = activityAssembler;
        }

        public override SkillDto DomainEntityToDto(ISkill entity)
        {
            SkillDto skillDto = new SkillDto
                                        {
                                            Name = entity.Name,
                                            Description = entity.Description,
                                            Resolution = entity.DefaultResolution,
                                            DisplayColor = new ColorDto(entity.DisplayColor),
                                            SkillType = entity.SkillType.Description.Name,
                                            Activity = _activityAssembler.DomainEntityToDto(entity.Activity),
                                            Id = entity.Id,
											WorkloadIdCollection = entity.WorkloadCollection.Select(w => w.Id.GetValueOrDefault()).ToArray()
                                        };
            return skillDto;
        }

        public override ISkill DtoToDomainEntity(SkillDto dto)
        {
            ISkill skill = null;
            if (dto.Id.HasValue)
            {
                skill = _skillRepository.Get(dto.Id.Value);
            }
            return skill;
        }
    }
}