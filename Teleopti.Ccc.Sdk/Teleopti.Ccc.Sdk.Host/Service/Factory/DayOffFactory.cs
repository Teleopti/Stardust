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
    internal static class DayOffFactory
    {
        internal static ICollection<DayOffInfoDto> GetDayOffs(LoadOptionDto loadOptionDto)
        {
            ICollection<DayOffInfoDto> dtoList;
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IRepositoryFactory repositoryFactory = new RepositoryFactory();
                IDayOffTemplateRepository repository = repositoryFactory.CreateDayOffRepository(unitOfWork);

                IList<IDayOffTemplate> daysOffList;
                if (loadOptionDto.LoadDeleted)
                {
                    using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                    {
                        daysOffList = repository.FindAllDayOffsSortByDescription();
                    }
                }
                else
                {
                    daysOffList = repository.FindAllDayOffsSortByDescription();
                }

                dtoList = new DayOffAssembler(null).DomainEntitiesToDtos(daysOffList).ToList();
            }

            return dtoList;
        }
    }
}