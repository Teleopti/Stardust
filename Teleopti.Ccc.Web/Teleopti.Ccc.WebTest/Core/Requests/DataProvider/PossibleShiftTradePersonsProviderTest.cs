﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
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
		private IPersonSelectorReadOnlyRepository personSelectorReadOnlyRepository;
		private ITeam myTeam;

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
			personSelectorReadOnlyRepository = MockRepository.GenerateMock<IPersonSelectorReadOnlyRepository>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Expect(l => loggedOnUser.CurrentUser()).Return(currentUser);

			target = new PossibleShiftTradePersonsProvider(personRepository, shiftTradeValidator, loggedOnUser, permissionProvider, personSelectorReadOnlyRepository);
		}

		[Test]
		public void ShouldNotReturnMeAsPossiblePersonToTradeShiftWith()
		{
			var currentUserGuids = new PersonSelectorShiftTrade { PersonId = currentUser.Id.Value, TeamId = myTeam.Id, SiteId = Guid.NewGuid(), BusinessUnitId = Guid.NewGuid() };
			var date = DateOnly.Today;
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = date, LoadOnlyMyTeam = true };

			personSelectorReadOnlyRepository.Expect(rep => rep.GetPersonForShiftTrade(data.ShiftTradeDate, myTeam.Id))
											.Return(new List<IAuthorizeOrganisationDetail> { currentUserGuids });

			personRepository.Expect(rep => rep.FindPeople(new List<Guid>())).Return(new Collection<IPerson>());

			target.RetrievePersons(data).Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnPossiblePersonsToTradeShiftWith()
		{
			var personInMyTeam = new Person();
			personInMyTeam.SetId(Guid.NewGuid());
			var personInMyTeamGuids = new PersonSelectorShiftTrade { PersonId = personInMyTeam.Id.Value, TeamId = myTeam.Id, SiteId = Guid.NewGuid(), BusinessUnitId = Guid.NewGuid() };
			var date = DateOnly.Today;
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = date, LoadOnlyMyTeam = true };

			personSelectorReadOnlyRepository.Expect(rep => rep.GetPersonForShiftTrade(data.ShiftTradeDate, myTeam.Id))
											.Return(new List<IAuthorizeOrganisationDetail> { personInMyTeamGuids });
			permissionProvider.Expect(
				perm =>
				perm.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate,
				                                     personInMyTeamGuids)).Return(true);
			personRepository.Expect(rep => rep.FindPeople(new[] { personInMyTeamGuids.PersonId }))
							.Return(new Collection<IPerson>(new List<IPerson> { personInMyTeam }));
			shiftTradeValidator.Expect(
				val => val.Validate(new ShiftTradeAvailableCheckItem(data.ShiftTradeDate, currentUser, personInMyTeam)))
							   .Return(new ShiftTradeRequestValidationResult(true));
			target.RetrievePersons(data).First().Should().Be.SameInstanceAs(personInMyTeam);
		}

		[Test]
		public void ShouldFilterPersonsWithNoPermissionToViewSchedules()
		{
			var validAgent = new Person();
			validAgent.SetId(Guid.NewGuid());
			var invalidAgent = new Person();
			invalidAgent.SetId(Guid.NewGuid());

			var validAgentGuids = new PersonSelectorShiftTrade { PersonId = validAgent.Id.Value, TeamId = myTeam.Id, SiteId = Guid.NewGuid(), BusinessUnitId = Guid.NewGuid() };
			var invalidAgentGuids = new PersonSelectorShiftTrade { PersonId = invalidAgent.Id.Value, TeamId = myTeam.Id, SiteId = Guid.NewGuid(), BusinessUnitId = Guid.NewGuid() };
			var date = DateOnly.Today;
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = date, LoadOnlyMyTeam = true };

			personSelectorReadOnlyRepository.Expect(rep => rep.GetPersonForShiftTrade(data.ShiftTradeDate, myTeam.Id))
											.Return(new List<IAuthorizeOrganisationDetail> { validAgentGuids, invalidAgentGuids });
			permissionProvider.Expect(
				perm =>
				perm.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate,
													 validAgentGuids)).Return(true);
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

			result.Should().Contain(validAgent);
			result.Should().Not.Contain(invalidAgent);
		}
	}
}