using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetSiteByIdQueryHandler : IHandleQuery<GetSiteByIdQueryDto, ICollection<SiteDto>>
    {
        private readonly IAssembler<ISite, SiteDto> _assembler;
        private readonly ISiteRepository _siteRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public GetSiteByIdQueryHandler(IAssembler<ISite, SiteDto> assembler, ISiteRepository siteRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _siteRepository = siteRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public ICollection<SiteDto> Handle(GetSiteByIdQueryDto query)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                {
                    var foundSite = _siteRepository.Get(query.SiteId);
                    if (foundSite == null) return new List<SiteDto>();
                    return new[] {_assembler.DomainEntityToDto(foundSite)};
                }
            }
        }
    }
}
