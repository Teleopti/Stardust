using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    internal static class OrganizationFactory
    {
        internal static IPersonCollection CreatePersonCollectionLight(IUnitOfWork unitOfWork, string functionPath, DateOnly queryDateTime)
        {
            var rep = new PersonRepository(new ThisUnitOfWork(unitOfWork));
                ICollection<IPerson> coll =
                    rep.FindPeopleInOrganizationLight(new DateOnlyPeriod(queryDateTime, queryDateTime));
                IPersonCollection ret = new PersonCollection(functionPath, coll, queryDateTime);
            
            return ret;
        }

        internal static ITeamCollection CreateTeamCollectionLight(IUnitOfWork unitOfWork, string functionPath, DateOnly queryDateTime)
        {
            var rep = new TeamRepository(unitOfWork);
            var coll = rep.LoadAll();
            var ret = new TeamCollection(functionPath, coll, queryDateTime);

            return ret;
        }
    }
}
