using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class TeamAssembler:Assembler<ITeam, TeamDto>
    {
        private readonly ITeamRepository _teamRepository;

        public TeamAssembler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public override TeamDto DomainEntityToDto(ITeam entity)
        {
            var teamDto = new TeamDto
                                        {
                                            Id = entity.Id,
                                            Description = entity.Description.Name,
                                            SiteAndTeam = entity.SiteAndTeam
                                        };
            return teamDto;
        }

        public override ITeam DtoToDomainEntity(TeamDto dto)
        {
            ITeam team = null;
            if (dto.Id.HasValue)
            {
                team = _teamRepository.Get(dto.Id.Value);
            }
            return team;
        }
    }
}