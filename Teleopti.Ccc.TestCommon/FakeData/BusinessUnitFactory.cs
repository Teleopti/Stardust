using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for BusinessUnit domain object
    /// </summary>
    public static class BusinessUnitFactory
    {
        private static BusinessUnit _businessUnitUsedInTest;

        /// <summary>
        /// Gets the business unit used in test.
        /// </summary>
        /// <value>The business unit used in test.</value>
        public static IBusinessUnit BusinessUnitUsedInTest
        {
            get
            {
                if (_businessUnitUsedInTest == null)
                {
                    _businessUnitUsedInTest = CreateSimpleBusinessUnit("Business unit used in test");
                    ((IEntity) _businessUnitUsedInTest).SetId(Guid.NewGuid());
                }

                return _businessUnitUsedInTest;
            }
        }

        public static void SetBusinessUnitUsedInTestToNull()
        {
            _businessUnitUsedInTest = null;
        }

        /// <summary>
        /// Creates a BusinessUnit
        /// </summary>
        /// <param name="name">Name of BusinessUnit</param>
        /// <returns></returns>
        public static BusinessUnit CreateSimpleBusinessUnit(string name)
        {
            BusinessUnit myBusinessUnit = new BusinessUnit(name);
            //typeof(Entity).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance)
            //    .SetValue(myBusinessUnit, Guid.NewGuid());
            return myBusinessUnit;
        }

        /// <summary>
        /// Creates A businessUnit without taking params
        /// </summary>
        public static BusinessUnit CreateSimpleBusinessUnit()
        {
            return CreateSimpleBusinessUnit("Sweden");
        }

        /// <summary>
        /// Creates a business unit aggregate list.
        /// </summary>
        /// <returns></returns>
        public static IList<BusinessUnit> CreateBusinessUnitTreeStructure()
        {
            IList<BusinessUnit> businessUnitList = new List<BusinessUnit>();
            businessUnitList.Add(CreateBusinessUnitWithSitesAndTeams());
            return businessUnitList;
        }

        /// <summary>
        /// Creates a business unit aggregate containing sites and teams.
        /// </summary>
        /// <returns></returns>
        public static BusinessUnit CreateBusinessUnitWithSitesAndTeams()
        {
            IList<ITeam> teams;
            return CreateBusinessUnitWithSitesAndTeams(out teams);
        }

        /// <summary>
        /// Creates a business unit aggregate containing sites and teams.
        /// </summary>
        /// <returns></returns>
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