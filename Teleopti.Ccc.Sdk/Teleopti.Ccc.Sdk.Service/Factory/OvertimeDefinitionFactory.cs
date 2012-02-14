using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
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
