using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    public static class OvertimeDefinitionFactory
    {
        public static ICollection<OvertimeDefinitionSetDto> GetOvertimeDefinitions(LoadOptionDto loadOptionDto)
        {
            ICollection<OvertimeDefinitionSetDto> dtoList;
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IRepositoryFactory repositoryFactory = new RepositoryFactory();
                IMultiplicatorDefinitionSetRepository repository =
                    repositoryFactory.CreateMultiplicatorDefinitionSetRepository(unitOfWork);
                IList<IMultiplicatorDefinitionSet> list;
                if (loadOptionDto.LoadDeleted)
                {
                    using(unitOfWork.DisableFilter(QueryFilter.Deleted))
                    {
                        list = repository.FindAllOvertimeDefinitions();
                    }
                }
                else
                {
                    list = repository.FindAllOvertimeDefinitions();
                }
                dtoList = new OvertimeDefinitionSetAssembler(null).DomainEntitiesToDtos(list).ToList();
            }
            return dtoList;
        }
    }
}
