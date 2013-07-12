﻿using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
    public static class PersonFactory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public static IList<IPerson> CreatePersonGraphCollection()
        {
            IList<IPerson> retList = new List<IPerson>();

            IBusinessUnit businessUnitGraph = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            businessUnitGraph.SetId(Guid.NewGuid());

            //Add a scorecard to one of the teams
			var _updatedOnDateTime = DateTime.Now;
            var scorecards = ScorecardFactory.CreateScorecardCollection(_updatedOnDateTime);
            businessUnitGraph.SiteCollection[0].TeamCollection[0].Scorecard = scorecards[0];

            //Agent with two periods
            IPerson person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("kalle", "kula");
            person.SetId(Guid.NewGuid());
            person.EmploymentNumber = "4321";
            person.PermissionInformation.SetDefaultTimeZone(
                //(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
                (TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time")));
            //FLE Standard Time
            IPersonContract personContract = PersonContractFactory.CreatePersonContract("PartTime", "xxx", "100");
            personContract.Contract.SetId(Guid.NewGuid());
            personContract.PartTimePercentage.SetId(Guid.NewGuid());

            //kolla "100"
            DateOnly date1 = new DateOnly(2007, 1, 1);
            IPersonPeriod personPeriod =
                PersonPeriodFactory.CreatePersonPeriod(date1, personContract, businessUnitGraph.SiteCollection[0].TeamCollection[0]);
            personPeriod.SetId(Guid.NewGuid());

            IPersonSkill skill = PersonSkillFactory.CreatePersonSkill("MySkill", 1);
            skill.Skill.SetId(Guid.NewGuid());
            personPeriod.AddPersonSkill(skill);

            IExternalLogOn login = ExternalLogOnFactory.CreateExternalLogOn();
            personPeriod.AddExternalLogOn(login);

            person.AddPersonPeriod(personPeriod); //what happens when overlapping periods

            DateOnly date2 = new DateOnly(2007, 2, 1);
            personPeriod = PersonPeriodFactory.CreatePersonPeriod(date2, personContract, businessUnitGraph.SiteCollection[1].TeamCollection[0]);
            personPeriod.SetId(Guid.NewGuid());

            businessUnitGraph.SiteCollection[0].SetId(Guid.NewGuid());
            businessUnitGraph.SiteCollection[1].SetId(Guid.NewGuid());
            businessUnitGraph.SiteCollection[0].TeamCollection[0].SetId(Guid.NewGuid());
            businessUnitGraph.SiteCollection[1].TeamCollection[0].SetId(Guid.NewGuid());
            person.AddPersonPeriod(personPeriod);
            person.TerminalDate = new DateOnly(2007, 2, 10);
            RaptorTransformerHelper.SetCreatedOn(person, DateTime.Now);
            retList.Add(person);

            //User
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.SetDefaultTimeZone(
                (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            RaptorTransformerHelper.SetCreatedOn(person, DateTime.Now);
            retList.Add(person);

            //Agent with one period with no terminal date set. No id for PersonPeriod
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("arne", "anka");
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.SetDefaultTimeZone(
                (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            personPeriod = PersonPeriodFactory.CreatePersonPeriod(date2, personContract, businessUnitGraph.SiteCollection[0].TeamCollection[1]);

            businessUnitGraph.SiteCollection[0].TeamCollection[1].SetId(Guid.NewGuid());
            person.AddPersonPeriod(personPeriod); //what happens when overlapping periods
            RaptorTransformerHelper.SetCreatedOn(person, DateTime.Now);
            retList.Add(person);

            // User with permission to view Performance Mangager reports (both windows and application logon info!)
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("John", "Doe");
            person.WindowsAuthenticationInfo = new WindowsAuthenticationInfo
                                                   {DomainName = "domain", WindowsLogOnName = "johnd"};

            person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
                                                       {
                                                           ApplicationLogOnName = "johnnied",
                                                           Password = "yupiiepwd"
                                                       };
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.AddApplicationRole(getApplicationRole("PM Users", true, false));
            RaptorTransformerHelper.SetCreatedOn(person, DateTime.Now);
            retList.Add(person);

            // User with permission to create AND view Performance Mangager reports
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("Belinda", "Bend");
            person.WindowsAuthenticationInfo = new WindowsAuthenticationInfo { DomainName = "domain", WindowsLogOnName = "belindab" };
            
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.AddApplicationRole(getApplicationRole("PM Users", false, true));
            RaptorTransformerHelper.SetCreatedOn(person, DateTime.Now);
            retList.Add(person);


            //UTC agent with two periods with no terminal date set.
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("oswald", "oblad");
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
            businessUnitGraph.SiteCollection[0].TeamCollection[1].SetId(Guid.NewGuid());

            personPeriod =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2009, 09, 22), personContract, businessUnitGraph.SiteCollection[0].TeamCollection[1]);
            person.AddPersonPeriod(personPeriod);

            personPeriod =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2009, 10, 22), personContract, businessUnitGraph.SiteCollection[0].TeamCollection[1]);
            businessUnitGraph.SiteCollection[0].TeamCollection[1].SetId(Guid.NewGuid());
            person.AddPersonPeriod(personPeriod);
            RaptorTransformerHelper.SetCreatedOn(person, DateTime.Now);
            retList.Add(person);


            // User with permission to create Performance Mangager reports (ONLY application logon info!)
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("Zacke", "zax");
            person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
                                                       {ApplicationLogOnName = "zackboy", Password = "zzpwd"};
            
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.AddApplicationRole(getApplicationRole("PM Users creators", false, true));
            person.PermissionInformation.AddApplicationRole(getApplicationRole("PM Users viewers", true, false));
            person.PermissionInformation.AddApplicationRole(getApplicationRole("No PM Users", false, false));
            RaptorTransformerHelper.SetCreatedOn(person, DateTime.Now);
            retList.Add(person);

            // User with Windows logon credentials but NO permission to Performance Mangager reports
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("Greg", "Gong");
            person.WindowsAuthenticationInfo = new WindowsAuthenticationInfo { DomainName = "Gdomain", WindowsLogOnName = "GregG" };

            person.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetCreatedOn(person, DateTime.Now);
            retList.Add(person);

            return retList;
        }

        private static IApplicationRole getApplicationRole(string roleName, bool isViewPermission, bool isCreatePermission)
        {
            IList<IApplicationFunction> applicationFunctions = new List<IApplicationFunction>();
            
            if (isViewPermission)
            {
                applicationFunctions.Add(ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                                               DefinedRaptorApplicationFunctionPaths.ViewPerformanceManagerReport));
            }

            if (isCreatePermission)
            {
                applicationFunctions.Add(ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                                               DefinedRaptorApplicationFunctionPaths.CreatePerformanceManagerReport));
            }
            
            IApplicationRole applicationRole = ApplicationRoleFactory.CreateRole(roleName, roleName);
            applicationRole.SetId(Guid.NewGuid());

            foreach (var applicationFunction in applicationFunctions)
            {
                applicationRole.AddApplicationFunction(applicationFunction);
            }

            return applicationRole;
        }
    }
}

