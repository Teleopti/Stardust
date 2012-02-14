﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    internal static class OrganizationFactory
    {
        internal static IPersonCollection CreatePersonCollectionLight(IUnitOfWork unitOfWork, string functionPath, DateOnly queryDateTime)
        {
            var rep = new PersonRepository(unitOfWork);
                ICollection<IPerson> coll =
                    rep.FindPeopleInOrganizationLight(new DateOnlyPeriod(queryDateTime, queryDateTime).ToDateTimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone));
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
