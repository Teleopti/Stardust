using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetAllScheduleTagsQueryHandler : IHandleQuery<GetAllScheduleTagsDto, ICollection<ScheduleTagDto>>
    {
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
            using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var scheduleTags = _scheduleTagRepository.FindAllScheduleTags();
                return _assembler.DomainEntitiesToDtos(scheduleTags).ToList();
            }
        }
    }
}
