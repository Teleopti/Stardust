﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
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
