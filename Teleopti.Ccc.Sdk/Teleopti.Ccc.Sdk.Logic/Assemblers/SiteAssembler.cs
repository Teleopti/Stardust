using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class SiteAssembler:Assembler<ISite, SiteDto>
    {
        private readonly ISiteRepository _siteRepository;

        public SiteAssembler(ISiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        public override SiteDto DomainEntityToDto(ISite entity)
        {
            var siteDto = new SiteDto
                              {
                                  Id = entity.Id,
                                  DescriptionName = entity.Description.Name
                              };
            return siteDto;
        }

        public override ISite DtoToDomainEntity(SiteDto dto)
        {
            ISite site = null;
            if (dto.Id.HasValue)
            {
                site = _siteRepository.Get(dto.Id.Value);
            }
            return site;
        }
    }
}