﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetAllScheduleTagsQueryHandler : IHandleQuery<GetAllScheduleTagsDto, ICollection<ScheduleTagDto>>
    {
        //Remember we should set a tag only when we modify the schedule.

        private readonly IScheduleTagRepository _scheduleTagRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IAssembler<IScheduleTag, ScheduleTagDto> _assembler;

        public GetAllScheduleTagsQueryHandler(IAssembler<IScheduleTag, ScheduleTagDto> assembler, IScheduleTagRepository scheduleTagRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
        {
            _scheduleTagRepository = scheduleTagRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _assembler = assembler;
        }

        public ICollection<ScheduleTagDto> Handle(GetAllScheduleTagsDto query)
        {
            using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
                var scheduleTags = _scheduleTagRepository.FindAllScheduleTags();
                return _assembler.DomainEntitiesToDtos(scheduleTags).ToList();
                //var export = _scheduleTagRepository.FindAllScheduleTags().Select(
                //                s =>
                //                new ScheduleTagDto() { Id = s.Id, Description = s.Description})
                //                .ToList();
                
            }
        }
    }
}
