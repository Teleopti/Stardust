﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class AvailableDataFactory
    {
        /// <summary>
        /// Creates available data with two business units two sites two teams and two persons.
        /// </summary>
        /// <returns></returns>
        public static AvailableData CreateAvailableDataWithTwoBusinessUnitsSitesTeamsPersons(out IList<IPerson> persons)
        {
            AvailableData availableData = new AvailableData();
            ApplicationRole applicationRole = ApplicationRoleFactory.CreateRole("ROLE", "ROLE");
            availableData.ApplicationRole = applicationRole;
            
            BusinessUnit unit1 = new BusinessUnit("Unit1");
            availableData.AddAvailableBusinessUnit(unit1);
            BusinessUnit unit2 = new BusinessUnit("Unit2");
            availableData.AddAvailableBusinessUnit(unit2);

            IPerson person1 = PersonFactory.CreatePerson("Person1");
            availableData.AddAvailablePerson(person1);
            IPerson person2 = PersonFactory.CreatePerson("Person2");
            availableData.AddAvailablePerson(person2);
            persons = new List<IPerson> { person1, person2 };

            Site site1 = new Site("Site1");
            availableData.AddAvailableSite(site1);
            Site site2 = new Site("Site2");
            availableData.AddAvailableSite(site2);

            Team team1 = TeamFactory.CreateSimpleTeam("Team1");
            site1.AddTeam(team1);
            availableData.AddAvailableTeam(team1);
            Team team2 = TeamFactory.CreateSimpleTeam("Team2");
            site2.AddTeam(team2);
            availableData.AddAvailableTeam(team2);

            return availableData;
        }

        /// <summary>
        /// Creates available data with two business units two sites two teams and two persons.
        /// </summary>
        /// <returns></returns>
        public static AvailableData CreateAvailableDataWithTwoBusinessUnitsSitesTeamsPersons()
        {
            IList<IPerson> persons;
            return CreateAvailableDataWithTwoBusinessUnitsSitesTeamsPersons(out persons);
        }

        /// <summary>
        /// Creates available data list with two available data instances. Each instances will
        /// have two business units, two sites, two teams and two persons.
        /// </summary>
        public static IList<IAvailableData> CreateAvailableDataList()
        {
            IList<IAvailableData> availableDatas = new List<IAvailableData>();
            IAvailableData data1 = CreateAvailableDataWithTwoBusinessUnitsSitesTeamsPersons();
            IAvailableData data2 = CreateAvailableDataWithTwoBusinessUnitsSitesTeamsPersons();
            availableDatas.Add(data1);
            availableDatas.Add(data2);
            return availableDatas;
        }
    }
}
