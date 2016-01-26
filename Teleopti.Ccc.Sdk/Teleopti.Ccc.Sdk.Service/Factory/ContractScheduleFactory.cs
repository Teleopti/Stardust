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
    internal static class ContractScheduleFactory
    {
        internal static ICollection<ContractScheduleDto> GetContractSchedules(LoadOptionDto loadOptionDto)
        {
            ICollection<ContractScheduleDto> dtoList;
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IRepositoryFactory repositoryFactory = new RepositoryFactory();
                IContractScheduleRepository repository = repositoryFactory.CreateContractScheduleRepository(unitOfWork);
                ICollection<IContractSchedule> list;
                if (loadOptionDto.LoadDeleted)
                {
                    using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                    {
                        list = repository.LoadAllAggregate();
                    }
                }
                else
                {
                    list = repository.LoadAllAggregate();
                }
                dtoList = new ContractScheduleAssembler(null).DomainEntitiesToDtos(list).ToList();
            }

            return dtoList;
        }
    }
}