using System;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
    public static class BusinessUnitFactory
    {
        public static IBusinessUnit CreateBusinessUnitWithSitesAndTeams()
        {
            IBusinessUnit swedenBusinessUnit = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit;
            Site danderydUnit = SiteFactory.CreateSimpleSite("Danderyd");
            Site strangnasUnit = SiteFactory.CreateSimpleSite("Strangnas");
            danderydUnit.AddTeam(TeamFactory.CreateSimpleTeam("Team Raptor"));
            danderydUnit.AddTeam(TeamFactory.CreateSimpleTeam("Team Pro"));
            strangnasUnit.AddTeam(TeamFactory.CreateSimpleTeam("Team CCC"));
            swedenBusinessUnit.AddSite(danderydUnit);
            swedenBusinessUnit.AddSite(strangnasUnit);
            return swedenBusinessUnit;
        }

        public static IBusinessUnit CreateSimpleBusinessUnit(string name)
        {
            IBusinessUnit myBusinessUnit = new BusinessUnit(name);
            myBusinessUnit.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetUpdatedOn(myBusinessUnit, DateTime.Now);

            return myBusinessUnit;
        }
    }
}