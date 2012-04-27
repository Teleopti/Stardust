﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetTeamByIdQueryHandler : IHandleQuery<GetTeamByIdQueryDto, ICollection<TeamDto>>
    {
        private readonly IAssembler<ITeam, TeamDto> _assembler;
        private readonly ITeamRepository _teamRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public GetTeamByIdQueryHandler(IAssembler<ITeam, TeamDto> assembler, ITeamRepository teamRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _teamRepository = teamRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<TeamDto> Handle(GetTeamByIdQueryDto query)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                {
                    var foundTeam = _teamRepository.Get(query.TeamId);
                    if (foundTeam == null) return new List<TeamDto>();
                    return new[] { _assembler.DomainEntityToDto(foundTeam) };
                }
            }
        }
    }
}
