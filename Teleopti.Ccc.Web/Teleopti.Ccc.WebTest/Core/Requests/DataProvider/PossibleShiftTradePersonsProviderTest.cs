using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory;


namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture, MyTimeWebTest]
	public class PossibleShiftTradePersonsProviderTest: IIsolateSystem
	{
		private IPerson currentUser;
		private ITeam myTeam;
		public ISettingsPersisterAndProvider<NameFormatSettings> NameFormatSettingsProvider;
		public PossibleShiftTradePersonsProvider Target;

		public FakePersonRepository PersonRepository;
		public FakePermissionProvider PermissionProvider;
		public FakePeopleForShiftTradeFinder PeopleForShiftTradeFinder;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePeopleForShiftTradeFinder>().For<IPeopleForShiftTradeFinder>();
			isolate.UseTestDouble<PossibleShiftTradePersonsProvider>().For<IPossibleShiftTradePersonsProvider>();
			
			myTeam = new Team();
			myTeam.SetId(Guid.NewGuid());
			currentUser = new Person();
			currentUser.SetId(Guid.NewGuid());
			currentUser.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, myTeam));
		}

		[Test]
		public void ShouldNotReturnMeAsPossiblePersonToTradeShiftWith()
		{
			var personGuids = new PersonSelectorShiftTrade
			{
				PersonId = currentUser.Id.Value,
				TeamId = myTeam.Id,
				SiteId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid()
			};
			PeopleForShiftTradeFinder.Has(personGuids);

			var date = DateOnly.Today.AddDays(2);
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = date,
				TeamIdList = new List<Guid> {myTeam.Id.GetValueOrDefault()}
			};

			var result = Target.RetrievePersons(data);

			result.Date.Should().Be.EqualTo(data.ShiftTradeDate);
			result.Persons.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnPossiblePersonToTradeShiftBySearchNameAndNameFormat()
		{
			var person = PersonFactory.CreatePersonWithGuid("agent", "agent");
			PersonRepository.Has(person);

			var personGuids = new PersonSelectorShiftTrade
			{
				PersonId = person.Id.Value,
				TeamId = myTeam.Id,
				SiteId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid()
			};
			PeopleForShiftTradeFinder.Has(personGuids);

			var date = DateOnly.Today.AddDays(2);
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = date,
				TeamIdList = new List<Guid> { myTeam.Id.GetValueOrDefault() }
			};

			NameFormatSettingsProvider.Persist(new NameFormatSettings { NameFormatId = (int)NameFormatSetting.LastNameThenFirstName });

			var result = Target.RetrievePersons(data);

			result.Date.Should().Be.EqualTo(data.ShiftTradeDate);
			result.Persons.Count().Should().Be(1);
			result.Persons.FirstOrDefault().Id.Should().Be(personGuids.PersonId);
		}


		[Test]
		public void ShouldReturnPossiblePersonsToTradeShiftWithForOneTeam()
		{
			var personInMyTeam = PersonFactory.CreatePersonWithGuid("agent", "agent");
			personInMyTeam.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personInMyTeam.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personInMyTeam.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			PersonRepository.Has(personInMyTeam);

			var personGuids = new PersonSelectorShiftTrade
			{
				PersonId = personInMyTeam.Id.Value,
				TeamId = myTeam.Id,
				SiteId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid()
			};

			PeopleForShiftTradeFinder.Has(personGuids);

			var date = DateOnly.Today.AddDays(2);
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = date,
				TeamIdList = new List<Guid> { myTeam.Id.GetValueOrDefault() }
			};
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate, currentUser);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate, personInMyTeam);

			var result = Target.RetrievePersons(data);

			result.Persons.First().Should().Be.SameInstanceAs(personInMyTeam);
			result.Persons.Count().Should().Be(1);
			result.Persons.FirstOrDefault().Id.Should().Be(personGuids.PersonId);
		}

		[Test]
		public void ShouldReturnPossiblePersonsToTradeShiftWithWhenSelectedAllTeams()
		{
			var personInMyTeam = PersonFactory.CreatePersonWithGuid("agent", "agent");
			personInMyTeam.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personInMyTeam.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personInMyTeam.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			PersonRepository.Has(personInMyTeam);

			var personGuids = new PersonSelectorShiftTrade
			{
				PersonId = personInMyTeam.Id.Value,
				TeamId = myTeam.Id,
				SiteId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid()
			};
			PeopleForShiftTradeFinder.Has(personGuids);

			var date = DateOnly.Today.AddDays(2);
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = date, TeamIdList = new List<Guid> { myTeam.Id.Value } };

			var result = Target.RetrievePersons(data);

			result.Persons.First().Should().Be.SameInstanceAs(personInMyTeam);
			result.Persons.Count().Should().Be(1);
			result.Persons.FirstOrDefault().Id.Should().Be(personGuids.PersonId);
		}

		[Test]
		public void ShouldFilterPersonsWithNoPermissionToViewSchedules()
		{
			var validAgent = PersonFactory.CreatePersonWithGuid("agent", "valid");
			validAgent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			validAgent.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			validAgent.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			PersonRepository.Has(validAgent);

			var invalidAgent = PersonFactory.CreatePersonWithGuid("NoShiftTrade", "invalid");
			PersonRepository.Has(invalidAgent);

			var validAgentGuids = new PersonSelectorShiftTrade
			{
				PersonId = validAgent.Id.Value,
				TeamId = myTeam.Id,
				SiteId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid()
			};
			PeopleForShiftTradeFinder.Has(validAgentGuids);

			var invalidAgentGuids = new PersonSelectorShiftTrade
			{
				PersonId = invalidAgent.Id.Value,
				TeamId = myTeam.Id,
				SiteId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid()
			};
			PeopleForShiftTradeFinder.Has(invalidAgentGuids);

			var date = DateOnly.Today.AddDays(2);
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = date,
				TeamIdList = new List<Guid>() { myTeam.Id.GetValueOrDefault() }
			};

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate, currentUser);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate, validAgent);

			var result = Target.RetrievePersons(data);

			result.Persons.Should().Contain(validAgent);
			result.Persons.Should().Not.Contain(invalidAgent);
		}

		[Test]
		public void ShouldFilterPersonsOpenPeriodForwardLongerToViewSchedules()
		{
			Now.Is(new DateTime(2017, 9, 1, 8, 8, 8, DateTimeKind.Utc));
			var validAgent = PersonFactory.CreatePersonWithGuid("agent", "valid");
			validAgent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			validAgent.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			validAgent.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			PersonRepository.Has(validAgent);

			var invalidAgent = PersonFactory.CreatePersonWithGuid("NoShiftTrade", "invalid");
			invalidAgent.SetId(Guid.NewGuid());
			invalidAgent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			invalidAgent.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			invalidAgent.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(3, 99);
			PersonRepository.Has(invalidAgent);

			var validAgentGuids = new PersonSelectorShiftTrade
			{
				PersonId = validAgent.Id.Value,
				TeamId = myTeam.Id,
				SiteId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid()
			};
			PeopleForShiftTradeFinder.Has(validAgentGuids);

			var invalidAgentGuids = new PersonSelectorShiftTrade
			{
				PersonId = invalidAgent.Id.Value,
				TeamId = myTeam.Id,
				SiteId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid()
			};
			PeopleForShiftTradeFinder.Has(invalidAgentGuids);

			var date = new DateOnly(Now.UtcDateTime()).AddDays(2);
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = date,
				TeamIdList = new List<Guid>() { myTeam.Id.GetValueOrDefault() }
			};

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate, currentUser);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate, validAgent);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate, invalidAgent);

			var result = Target.RetrievePersons(data);

			result.Persons.Should().Contain(validAgent);
			result.Persons.Should().Not.Contain(invalidAgent);
		}

		[Test]
		public void ShouldReturnPossiblePersonsToTradeShiftWhenSearchNameText()
		{
			var person1InMyTeam = PersonFactory.CreatePersonWithGuid("1", "person");
			PersonRepository.Has(person1InMyTeam);

			var person2InMyTeam = PersonFactory.CreatePersonWithGuid("2", "person");
			person2InMyTeam.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person2InMyTeam.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			person2InMyTeam.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			PersonRepository.Has(person2InMyTeam);

			var person2InMyTeamGuids = new PersonSelectorShiftTrade
			{
				PersonId = person2InMyTeam.Id.Value,
				TeamId = myTeam.Id,
				SiteId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid()
			};
			PeopleForShiftTradeFinder.Has(person2InMyTeamGuids);

			var date = DateOnly.Today.AddDays(2);
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = date,
				TeamIdList = new List<Guid> { myTeam.Id.GetValueOrDefault() },
				SearchNameText = person2InMyTeam.Name.FirstName
			};

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate, currentUser);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ShiftTradeDate, person2InMyTeam);

			var result = Target.RetrievePersons(data);
			result.Persons.Count().Should().Be(1);
			result.Persons.First().Should().Be.SameInstanceAs(person2InMyTeam);
		}
	}
}