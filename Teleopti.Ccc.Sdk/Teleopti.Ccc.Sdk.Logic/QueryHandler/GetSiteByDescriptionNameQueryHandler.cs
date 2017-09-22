using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetSiteByDescriptionNameQueryHandler : IHandleQuery<GetSiteByDescriptionNameQueryDto, ICollection<SiteDto>>
    {
        private readonly IAssembler<ISite, SiteDto> _assembler;
        private readonly ISiteRepository _siteRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

        public GetSiteByDescriptionNameQueryHandler(IAssembler<ISite, SiteDto> assembler, ISiteRepository siteRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _siteRepository = siteRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<SiteDto> Handle(GetSiteByDescriptionNameQueryDto query)
        {
            using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
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
