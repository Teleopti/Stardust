using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    internal static class OrganizationFactory
    {
        internal static IPersonCollection CreatePersonCollectionLight(IUnitOfWork unitOfWork, string functionPath, DateOnly queryDateTime)
        {
            var rep = PersonRepository.DONT_USE_CTOR(new ThisUnitOfWork(unitOfWork), null, null);
                ICollection<IPerson> coll =
                    rep.FindAllAgentsLight(new DateOnlyPeriod(queryDateTime, queryDateTime));
                IPersonCollection ret = new PersonCollection(functionPath, coll, queryDateTime);
            
            return ret;
        }

        internal static ITeamCollection CreateTeamCollectionLight(IUnitOfWork unitOfWork, string functionPath, DateOnly queryDateTime)
        {
            var rep = TeamRepository.DONT_USE_CTOR(unitOfWork);
            var coll = rep.LoadAll();
            var ret = new TeamCollection(functionPath, coll, queryDateTime);

            return ret;
        }
    }
}
