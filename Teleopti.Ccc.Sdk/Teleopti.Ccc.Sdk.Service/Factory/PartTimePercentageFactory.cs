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
    internal static class PartTimePercentageFactory
    {
        internal static ICollection<PartTimePercentageDto> GetPartTimePercentages(LoadOptionDto loadOptionDto)
        {
            ICollection<PartTimePercentageDto> dtoList;
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IRepositoryFactory repositoryFactory = new RepositoryFactory();
                IPartTimePercentageRepository repository =
                    repositoryFactory.CreatePartTimePercentageRepository(unitOfWork);

                ICollection<IPartTimePercentage> list;
                if (loadOptionDto.LoadDeleted)
                {
                    using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                    {
                        list = repository.FindAllPartTimePercentageByDescription();
                    }
                }
                else
                {
                    list = repository.FindAllPartTimePercentageByDescription();
                }

                dtoList = new PartTimePercentageAssembler(null).DomainEntitiesToDtos(list).ToList();
            }

            return dtoList;
        }
    }
}