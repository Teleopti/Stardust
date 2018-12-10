using System;
using System.Collections.Generic;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
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
            person.SetEmploymentNumber("4321");
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

			person.AddPersonPeriod(personPeriod); //what happens when overlapping periods

            IPersonSkill skill = PersonSkillFactory.CreatePersonSkill("MySkill", 1);
            skill.Skill.SetId(Guid.NewGuid());
            person.AddSkill(skill,personPeriod);

            IExternalLogOn login = ExternalLogOnFactory.CreateExternalLogOn();
				person.AddExternalLogOn(login, personPeriod);

            DateOnly date2 = new DateOnly(2007, 2, 1);
            personPeriod = PersonPeriodFactory.CreatePersonPeriod(date2, personContract, businessUnitGraph.SiteCollection[1].TeamCollection[0]);
            personPeriod.SetId(Guid.NewGuid());

            businessUnitGraph.SiteCollection[0].SetId(Guid.NewGuid());
            businessUnitGraph.SiteCollection[1].SetId(Guid.NewGuid());
            businessUnitGraph.SiteCollection[0].TeamCollection[0].SetId(Guid.NewGuid());
            businessUnitGraph.SiteCollection[1].TeamCollection[0].SetId(Guid.NewGuid());
            person.AddPersonPeriod(personPeriod);
            person.TerminatePerson(new DateOnly(2007, 2, 10), new MockRepository().StrictMock<IPersonAccountUpdater>());
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
            retList.Add(person);

            //User
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.SetDefaultTimeZone(
                (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
            retList.Add(person);

            //Agent with one period with no terminal date set. No id for PersonPeriod
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("arne", "anka");
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.SetDefaultTimeZone(
                (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            personPeriod = PersonPeriodFactory.CreatePersonPeriod(date2, personContract, businessUnitGraph.SiteCollection[0].TeamCollection[1]);

            businessUnitGraph.SiteCollection[0].TeamCollection[1].SetId(Guid.NewGuid());
            person.AddPersonPeriod(personPeriod); //what happens when overlapping periods
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
            retList.Add(person);

            // User with permission to view Performance Mangager reports (both windows and application logon info!)
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("John", "Doe");
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.AddApplicationRole(getApplicationRole("PM Users", true, false));
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
            retList.Add(person);

            // User with permission to create AND view Performance Mangager reports
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("Belinda", "Bend");
            //person.AuthenticationInfo = new AuthenticationInfo { Identity = @"domain\belindab" };
            
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.AddApplicationRole(getApplicationRole("PM Users", false, true));
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
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
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
            retList.Add(person);


            // User with permission to create Performance Mangager reports (ONLY application logon info!)
            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("Zacke", "zax");
            
            
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.AddApplicationRole(getApplicationRole("PM Users creators", false, true));
            person.PermissionInformation.AddApplicationRole(getApplicationRole("PM Users viewers", true, false));
            person.PermissionInformation.AddApplicationRole(getApplicationRole("No PM Users", false, false));
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
            retList.Add(person);

            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("Greg", "Gong");

            person.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
            retList.Add(person);

            return retList;
        }

		 public static IList<IPerson> CreatePersonGraphCollectionWithInfinitStart()
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
			  person.SetEmploymentNumber("4321");
			  person.PermissionInformation.SetDefaultTimeZone(
				  //(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
					(TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time")));
			  //FLE Standard Time
			  IPersonContract personContract = PersonContractFactory.CreatePersonContract("PartTime", "xxx", "100");
			  personContract.Contract.SetId(Guid.NewGuid());
			  personContract.PartTimePercentage.SetId(Guid.NewGuid());

			  //kolla "100"
		  DateOnly date1 = new DateOnly(5051, 1, 1);
			  IPersonPeriod personPeriod =
					PersonPeriodFactory.CreatePersonPeriod(date1, personContract, businessUnitGraph.SiteCollection[0].TeamCollection[0]);
			  personPeriod.SetId(Guid.NewGuid());

			  person.AddPersonPeriod(personPeriod); //what happens when overlapping periods

			  IPersonSkill skill = PersonSkillFactory.CreatePersonSkill("MySkill", 1);
			  skill.Skill.SetId(Guid.NewGuid());
			  person.AddSkill(skill, personPeriod);

			  IExternalLogOn login = ExternalLogOnFactory.CreateExternalLogOn();
			  person.AddExternalLogOn(login, personPeriod);

			  businessUnitGraph.SiteCollection[0].SetId(Guid.NewGuid());
			  businessUnitGraph.SiteCollection[1].SetId(Guid.NewGuid());
			  businessUnitGraph.SiteCollection[0].TeamCollection[0].SetId(Guid.NewGuid());
			  businessUnitGraph.SiteCollection[1].TeamCollection[0].SetId(Guid.NewGuid());
			  RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
			  retList.Add(person);


			  return retList;
		  }

        private static IApplicationRole getApplicationRole(string roleName, bool isViewPermission, bool isCreatePermission)
        {
            IList<IApplicationFunction> applicationFunctions = new List<IApplicationFunction>();
            
            if (isViewPermission)
            {
                applicationFunctions.Add(ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
                                               DefinedRaptorApplicationFunctionPaths.ViewPerformanceManagerReport));
            }

            if (isCreatePermission)
            {
                applicationFunctions.Add(ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
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

