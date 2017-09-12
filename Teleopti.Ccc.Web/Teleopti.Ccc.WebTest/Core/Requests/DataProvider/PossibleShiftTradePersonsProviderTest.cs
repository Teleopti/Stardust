﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
    [TestFixture]
	public class PossibleShiftTradePersonsProviderTest
	{
		private IPossibleShiftTradePersonsProvider target;
		private IShiftTradeLightValidator shiftTradeValidator;
		private IPersonRepository personRepository;
		private IPermissionProvider permissionProvider;
		private IPerson currentUser;
		private IPersonForScheduleFinder personForScheduleFinder;
		private ITeam myTeam;
	    private ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettingsProvider;
		private IPeopleForShiftTradeFinder peopleForShiftTradeFinder;
		private ThisIsNow now;

		[SetUp]
		public void Setup()
		{
			myTeam = new Team();
			myTeam.SetId(Guid.NewGuid());
			currentUser = new Person();
			currentUser.SetId(Guid.NewGuid());
			currentUser.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, myTeam));
			shiftTradeValidator = MockRepository.GenerateMock<IShiftTradeLightValidator>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personForScheduleFinder = MockRepository.GenerateMock<IPersonForScheduleFinder>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var loggedOnUser = new FakeLoggedOnUser(currentUser);
			peopleForShiftTradeFinder = MockRepository.GenerateMock<IPeopleForShiftTradeFinder>();
			nameFormatSettingsProvider = new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository());
			now = new ThisIsNow(new DateTime(2017, 9, 1, 8, 8, 8, DateTimeKind.Utc));

			target = new PossibleShiftTradePersonsProvider(nameFormatSettingsProvider, new ShiftTradePersonProvider (personRepository, shiftTradeValidator, permissionProvider, personForScheduleFinder, loggedOnUser, peopleForShiftTradeFinder, now)
				,new FakeToggleManager());
		}

		[Test]
		public void ShouldNotReturnMeAsPossiblePersonToTradeShiftWith()
		{
			var currentUserGuids = new PersonSelectorShiftTrade
				{
					PersonId = currentUser.Id.Value, 
					TeamId = myTeam.Id, 
					SiteId = Guid.NewGuid(), 
					BusinessUnitId = Guid.NewGuid()
				};
			var date = DateOnly.Today.AddDays(2);
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = date,
				TeamIdList = new List<Guid> {myTeam.Id.GetValueOrDefault()}
			};

			personForScheduleFinder.Expect(rep => rep.GetPersonFor(data.ShiftTradeDate,data.TeamIdList , data.SearchNameText))
											.Return(new List<IPersonAuthorization> { currentUserGuids });

			personRepository.Expect(rep => rep.FindPeople(new List<Guid>())).Return(new Collection<IPerson>());

			var result = target.RetrievePersons(data);

			result.Date.Should().Be.EqualTo(data.ShiftTradeDate);
			result.Persons.Should().Be.Empty();
		}

        [Test]
        public void ShouldReturnPossiblePersonToTradeShiftBySearchNameAndNameFormat()
        {
            var currentUserGuids = new PersonSelectorShiftTrade
            {
                PersonId = currentUser.Id.Value,
                TeamId = myTeam.Id,
                SiteId = Guid.NewGuid(),
                BusinessUnitId = Guid.NewGuid()
            };
            var date = DateOnly.Today.AddDays(2);
            var data = new ShiftTradeScheduleViewModelData
            {
                ShiftTradeDate = date,
                TeamIdList = new List<Guid> { myTeam.Id.GetValueOrDefault() }
            };

            nameFormatSettingsProvider.Persist(new NameFormatSettings{ NameFormatId = (int)NameFormatSetting.LastNameThenFirstName});

            personForScheduleFinder.Expect(rep => rep.GetPersonFor(data.ShiftTradeDate, data.TeamIdList, data.SearchNameText, NameFormatSetting.LastNameThenFirstName ))
                                            .Return(new List<IPersonAuthorization> { currentUserGuids });

            personRepository.Expect(rep => rep.FindPeople(new List<Guid>())).Return(new Collection<IPerson>());

            var result = target.RetrievePersons(data);

            result.Date.Should().Be.EqualTo(data.ShiftTradeDate);       
        }


        [Test]
		public void ShouldReturnPossiblePersonsToTradeShiftWithForOneTeam()
		{
			var personInMyTeam = new Person();
			personInMyTeam.SetId(Guid.NewGuid());
			personInMyTeam.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personInMyTeam.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personInMyTeam.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			var personInMyTeamGuids = new PersonSelectorShiftTrade { PersonId = personInMyTeam.Id.Value, TeamId = myTeam.Id, SiteId = Guid.NewGuid(), BusinessUnitId = Guid.NewGuid() };
			var date = DateOnly.Today.AddDays(2);
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = date,
				TeamIdList = new List<Guid> {myTeam.Id.GetValueOrDefault()}
			};

			personForScheduleFinder.Expect(rep => rep.GetPersonFor(data.ShiftTradeDate, data.TeamIdList, data.SearchNameText))
											.Return(new List<IPersonAuthorization> { personInMyTeamGuids });
			permissionProvider.Expect(
				perm =>
				perm.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate,
				                                     personInMyTeamGuids)).Return(true);
			permissionProvider.Expect(perm => perm.IsPersonSchedulePublished(data.ShiftTradeDate, personInMyTeam)).Return(true);
			personRepository.Expect(rep => rep.FindPeople(new[] { personInMyTeamGuids.PersonId }))
							.Return(new Collection<IPerson>(new List<IPerson> { personInMyTeam }));
			shiftTradeValidator.Expect(
				val => val.Validate(new ShiftTradeAvailableCheckItem(data.ShiftTradeDate, currentUser, personInMyTeam)))
							   .Return(new ShiftTradeRequestValidationResult(true));
			
			var result = target.RetrievePersons(data);

			result.Persons.First().Should().Be.SameInstanceAs(personInMyTeam);
		}
		
		[Test]
		public void ShouldReturnPossiblePersonsToTradeShiftWithWhenSelectedAllTeams()
		{
			var personInMyTeam = new Person();
			personInMyTeam.SetId(Guid.NewGuid());
			personInMyTeam.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personInMyTeam.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personInMyTeam.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			var personInMyTeamGuids = new PersonSelectorShiftTrade { PersonId = personInMyTeam.Id.Value, TeamId = myTeam.Id, SiteId = Guid.NewGuid(), BusinessUnitId = Guid.NewGuid() };
			var date = DateOnly.Today.AddDays(2);
			var teamIds = new List<Guid>();
			teamIds.Add(myTeam.Id.Value);
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = date, TeamIdList = teamIds };

			personForScheduleFinder.Expect(rep => rep.GetPersonFor(data.ShiftTradeDate, data.TeamIdList, data.SearchNameText))
											.Return(new List<IPersonAuthorization> { personInMyTeamGuids });
			permissionProvider.Expect(
				perm =>
				perm.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate,
				                                     personInMyTeamGuids)).Return(true);
			permissionProvider.Expect(perm => perm.IsPersonSchedulePublished(data.ShiftTradeDate, personInMyTeam)).Return(true);
			personRepository.Expect(rep => rep.FindPeople(new[] { personInMyTeamGuids.PersonId }))
							.Return(new Collection<IPerson>(new List<IPerson> { personInMyTeam }));
			shiftTradeValidator.Expect(
				val => val.Validate(new ShiftTradeAvailableCheckItem(data.ShiftTradeDate, currentUser, personInMyTeam)))
							   .Return(new ShiftTradeRequestValidationResult(true));
			
			var result = target.RetrievePersons(data);

			result.Persons.First().Should().Be.SameInstanceAs(personInMyTeam);
		}
		
		[Test]
		public void ShouldFilterPersonsWithNoPermissionToViewSchedules()
		{
			var validAgent = new Person();
			validAgent.SetId(Guid.NewGuid());
			validAgent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			validAgent.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			validAgent.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			var invalidAgent = new Person();
			invalidAgent.SetId(Guid.NewGuid());

			var validAgentGuids = new PersonSelectorShiftTrade { PersonId = validAgent.Id.Value, TeamId = myTeam.Id, SiteId = Guid.NewGuid(), BusinessUnitId = Guid.NewGuid() };
			var invalidAgentGuids = new PersonSelectorShiftTrade { PersonId = invalidAgent.Id.Value, TeamId = myTeam.Id, SiteId = Guid.NewGuid(), BusinessUnitId = Guid.NewGuid() };
			var date = DateOnly.Today.AddDays(2);
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = date, TeamIdList = new List<Guid>(){myTeam.Id.GetValueOrDefault()}};

			personForScheduleFinder.Expect(rep => rep.GetPersonFor(data.ShiftTradeDate,data.TeamIdList,data.SearchNameText))
											.Return(new List<IPersonAuthorization> { validAgentGuids, invalidAgentGuids });
			permissionProvider.Expect(
				perm =>
				perm.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate,
													 validAgentGuids)).Return(true);
			permissionProvider.Expect(perm => perm.IsPersonSchedulePublished(data.ShiftTradeDate, validAgent)).Return(true);

			permissionProvider.Expect(
				perm =>
				perm.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate,
													 invalidAgentGuids)).Return(false);
			personRepository.Expect(rep => rep.FindPeople(new[] { validAgentGuids.PersonId }))
							.Return(new Collection<IPerson>(new List<IPerson> { validAgent }));
			shiftTradeValidator.Expect(
				val => val.Validate(new ShiftTradeAvailableCheckItem(data.ShiftTradeDate, currentUser, validAgent)))
							   .Return(new ShiftTradeRequestValidationResult(true));
			var result = target.RetrievePersons(data);

			result.Persons.Should().Contain(validAgent);
			result.Persons.Should().Not.Contain(invalidAgent);
		}

		[Test]
		public void ShouldFilterPersonsOpenPeriodForwardLongerToViewSchedules()
		{
			var validAgent = new Person();
			validAgent.SetId(Guid.NewGuid());
			validAgent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			validAgent.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			validAgent.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			var invalidAgent = new Person();
			invalidAgent.SetId(Guid.NewGuid());
			invalidAgent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			invalidAgent.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			invalidAgent.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(3, 99);

			var validAgentGuids = new PersonSelectorShiftTrade { PersonId = validAgent.Id.Value, TeamId = myTeam.Id, SiteId = Guid.NewGuid(), BusinessUnitId = Guid.NewGuid() };
			var invalidAgentGuids = new PersonSelectorShiftTrade { PersonId = invalidAgent.Id.Value, TeamId = myTeam.Id, SiteId = Guid.NewGuid(), BusinessUnitId = Guid.NewGuid() };
			var date = new DateOnly(now.UtcDateTime()).AddDays(2);
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = date, TeamIdList = new List<Guid>(){myTeam.Id.GetValueOrDefault()}};

			personForScheduleFinder.Expect(rep => rep.GetPersonFor(data.ShiftTradeDate,data.TeamIdList,data.SearchNameText))
											.Return(new List<IPersonAuthorization> { validAgentGuids, invalidAgentGuids });
			permissionProvider.Expect(perm => perm.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate,
													 validAgentGuids)).Return(true);
			permissionProvider.Expect(perm => perm.IsPersonSchedulePublished(data.ShiftTradeDate, validAgent)).Return(true);

			permissionProvider.Expect(perm => perm.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate,
													 invalidAgentGuids)).Return(true);
			permissionProvider.Expect(perm => perm.IsPersonSchedulePublished(data.ShiftTradeDate, invalidAgent)).Return(true);
			personRepository.Expect(rep => rep.FindPeople(new[] { validAgentGuids.PersonId, invalidAgentGuids.PersonId }))
							.Return(new Collection<IPerson>(new List<IPerson> { validAgent, invalidAgent }));
			shiftTradeValidator.Expect(val => val.Validate(new ShiftTradeAvailableCheckItem(data.ShiftTradeDate, currentUser, validAgent)))
							   .Return(new ShiftTradeRequestValidationResult(true));
			shiftTradeValidator.Expect(val => val.Validate(new ShiftTradeAvailableCheckItem(data.ShiftTradeDate, currentUser, invalidAgent)))
							   .Return(new ShiftTradeRequestValidationResult(true));

			var result = target.RetrievePersons(data);

			result.Persons.Should().Contain(validAgent);
			result.Persons.Should().Not.Contain(invalidAgent);
		}

		[Test]
		public void ShouldReturnPossiblePersonsToTradeShiftWhenSearchNameText()
		{
			var person1InMyTeam = new Person();
			person1InMyTeam.WithName(new Name("1", "person"));
			person1InMyTeam.SetId(Guid.NewGuid());

			var person2InMyTeam = new Person();
			person2InMyTeam.WithName(new Name("2","person"));
			person2InMyTeam.SetId(Guid.NewGuid());
			var person2InMyTeamGuids = new PersonSelectorShiftTrade { PersonId = person2InMyTeam.Id.Value, TeamId = myTeam.Id, SiteId = Guid.NewGuid(), BusinessUnitId = Guid.NewGuid() };
			person2InMyTeam.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person2InMyTeam.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			person2InMyTeam.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			
			var date = DateOnly.Today.AddDays(2);
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = date,
				TeamIdList = new List<Guid> { myTeam.Id.GetValueOrDefault() },
				SearchNameText = person2InMyTeam.Name.FirstName
			};

			personForScheduleFinder.Expect(rep => rep.GetPersonFor(data.ShiftTradeDate, data.TeamIdList, data.SearchNameText))
											.Return(new List<IPersonAuthorization> { person2InMyTeamGuids });
			permissionProvider.Expect(
				perm =>
				perm.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate,
													 person2InMyTeamGuids)).Return(true);
			permissionProvider.Expect(perm => perm.IsPersonSchedulePublished(data.ShiftTradeDate, person2InMyTeam)).Return(true);
			personRepository.Expect(rep => rep.FindPeople(new[] { person2InMyTeamGuids.PersonId }))
							.Return(new Collection<IPerson>(new List<IPerson> { person2InMyTeam }));
			shiftTradeValidator.Expect(
				val => val.Validate(new ShiftTradeAvailableCheckItem(data.ShiftTradeDate, currentUser, person2InMyTeam)))
							   .Return(new ShiftTradeRequestValidationResult(true));

			var result = target.RetrievePersons(data);

			result.Persons.First().Should().Be.SameInstanceAs(person2InMyTeam);
		}
	}
}