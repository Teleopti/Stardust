﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetTeamByDescriptionNameQueryHandler : IHandleQuery<GetTeamByDescriptionNameQueryDto, ICollection<TeamDto>>
    {
        private readonly IAssembler<ITeam, TeamDto> _assembler;
        private readonly ITeamRepository _teamRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public GetTeamByDescriptionNameQueryHandler(IAssembler<ITeam, TeamDto> assembler, ITeamRepository teamRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _teamRepository = teamRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public ICollection<TeamDto> Handle(GetTeamByDescriptionNameQueryDto query)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                {
                    var memberList = new List<ITeam>();
                    var foundTeams = _teamRepository.FindTeamByDescriptionName(query.DescriptionName);
                    memberList.AddRange(foundTeams);
                    return _assembler.DomainEntitiesToDtos(memberList).ToList();
                }
            }
        }
    }
}
