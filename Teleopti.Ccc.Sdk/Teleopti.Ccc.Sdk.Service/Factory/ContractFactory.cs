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
    internal static class ContractFactory
    {
        internal static ICollection<ContractDto> GetContracts(LoadOptionDto loadOptionDto)
        {
            ICollection<ContractDto> dtoList;
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IRepositoryFactory repositoryFactory = new RepositoryFactory();
                IContractRepository repository = repositoryFactory.CreateContractRepository(unitOfWork);
                ICollection<IContract> list;
                if (loadOptionDto.LoadDeleted)
                {
                    using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                    {
                        list = repository.FindAllContractByDescription();
                    }
                }
                else
                {
                    list = repository.FindAllContractByDescription();
                }
                dtoList = new ContractAssembler(null).DomainEntitiesToDtos(list).ToList();
            }

            return dtoList;
        }
    }
}