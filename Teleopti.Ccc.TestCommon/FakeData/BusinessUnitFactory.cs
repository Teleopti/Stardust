﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class BusinessUnitFactory
    {
        private static Lazy<IBusinessUnit> _businessUnitUsedInTest = new Lazy<IBusinessUnit>(()=>CreateSimpleBusinessUnit("Business unit used in test").WithId());

	    public static void CreateNewBusinessUnitUsedInTest()
	    {
		    _businessUnitUsedInTest = new Lazy<IBusinessUnit>(() => CreateSimpleBusinessUnit("Business unit used in test").WithId());
	    }

        public static IBusinessUnit BusinessUnitUsedInTest
        {
            get
            {
	            return _businessUnitUsedInTest.Value;
            }
        }

        public static BusinessUnit CreateSimpleBusinessUnit(string name)
        {
            BusinessUnit myBusinessUnit = new BusinessUnit(name);
            return myBusinessUnit;
        }

		public static BusinessUnit CreateWithId(string name)
		{
			var businessUnit = new BusinessUnit(name);
			businessUnit.SetId(Guid.NewGuid());
			return businessUnit;
		}

        public static BusinessUnit CreateSimpleBusinessUnit()
        {
            return CreateSimpleBusinessUnit("Sweden");
        }

        public static BusinessUnit CreateBusinessUnitWithSitesAndTeams()
        {
            IList<ITeam> teams;
            return CreateBusinessUnitWithSitesAndTeams(out teams);
        }

        public static BusinessUnit CreateBusinessUnitWithSitesAndTeams(out IList<ITeam> teams)
        {
            BusinessUnit swedenBusinessUnit = new BusinessUnit("Sweden");
            Site danderydUnit = SiteFactory.CreateSimpleSite("Danderyd");
            Site strangnasUnit = SiteFactory.CreateSimpleSite("Strangnas");
            ITeam team1 = TeamFactory.CreateSimpleTeam("Team Raptor");
            ITeam team2 = TeamFactory.CreateSimpleTeam("Team Pro");
            ITeam team3 = TeamFactory.CreateSimpleTeam("Team CCC");
            danderydUnit.AddTeam(team1);
            danderydUnit.AddTeam(team2);
            strangnasUnit.AddTeam(team3);
            teams = new List<ITeam>{team1, team2, team3};
            swedenBusinessUnit.AddSite(danderydUnit);
            swedenBusinessUnit.AddSite(strangnasUnit);
            return swedenBusinessUnit;
        }
    }
}