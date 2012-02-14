using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    internal static class ActivityTypeFactory
    {
        internal static ICollection<ActivityDto> GetActivities(LoadOptionDto loadOptionDto)
        {
            var dtoList = new List<ActivityDto>();

            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ActivityRepository rep = new ActivityRepository(unitOfWork);
                IList<IActivity> activityList;
                if (loadOptionDto.LoadDeleted)
                {
                    using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                    {
                        activityList = rep.LoadAllSortByName();
                    }
                }
                else
                {
                    activityList = rep.LoadAllSortByName();
                }

                var activityAssembler = new ActivityAssembler(rep);
                dtoList.AddRange(activityAssembler.DomainEntitiesToDtos(activityList));
            }

            return dtoList;
        }
    }
}