using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.QueryHandler
{
    public class GetSiteByDescriptionNameQueryHandler : IHandleQuery<GetSiteByDescriptionNameQueryDto, ICollection<SiteDto>>
    {
        private readonly IAssembler<ISite, SiteDto> _assembler;
        private readonly ISiteRepository _siteRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public GetSiteByDescriptionNameQueryHandler(IAssembler<ISite, SiteDto> assembler, ISiteRepository siteRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _siteRepository = siteRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public ICollection<SiteDto> Handle(GetSiteByDescriptionNameQueryDto query)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                {
                    var memberList = new List<ISite>();
                    var foundSites = _siteRepository.FindSiteByDescriptionName(query.DescriptionName);
                    memberList.AddRange(foundSites);
                    return _assembler.DomainEntitiesToDtos(memberList).ToList();
                }
            }
        }
    }
}
