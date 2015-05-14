using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	[TestFixture]
	public class TeamSchedulePersonsProviderTest
	{
		private TeamSchedulePersonsProvider provider;
		private IPermissionProvider permissionProvider;
		private IPersonRepository personRepository;
		private IPersonForShiftTradeRepository personForShiftTradeRepository;
		private TeamScheduleViewModelData data;
		private readonly Guid teamId = Guid.NewGuid();

		[SetUp]
		public void SetUp()
		{
			
			data = new TeamScheduleViewModelData()
			{
				ScheduleDate = new DateOnly(2015,5,13),
				TeamIdList = new[] { teamId }
			};
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			personForShiftTradeRepository = MockRepository.GenerateMock<IPersonForShiftTradeRepository>();
			provider = new TeamSchedulePersonsProvider(permissionProvider, personForShiftTradeRepository, personRepository);
		}

		[Test]
		public void ShouldRetrievePersons()
		{
			personForShiftTradeRepository.Stub(
				x => x.GetPersonForShiftTrade(data.ScheduleDate, data.TeamIdList,null))
				.Return(new List<IAuthorizeOrganisationDetail>());

			permissionProvider.Stub(
				x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ScheduleDate,new PersonSelectorOrganization()))
				.Return(true);

			var ret = provider.RetrievePersons(data);
			ret.ToList().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRetrievePersonsFromTeam()
		{
			var person1 = PersonFactory.CreatePersonWithId();
			var person2 = PersonFactory.CreatePersonWithId();

			var personSelectorOrganization1 = new PersonSelectorOrganization {PersonId = person1.Id.GetValueOrDefault()};
			var personSelectorOrganization2 = new PersonSelectorOrganization {PersonId = person2.Id.GetValueOrDefault()};

			personForShiftTradeRepository.Stub(
				x => x.GetPersonForShiftTrade(data.ScheduleDate, data.TeamIdList, null))
				.Return(new IAuthorizeOrganisationDetail[]
				{
					personSelectorOrganization1,personSelectorOrganization2					
				});

			personRepository.Stub(
				x => x.FindPeople(new[] { personSelectorOrganization1.PersonId ,personSelectorOrganization2.PersonId}))
				.Return(new[] { person1,person2 });
			

			permissionProvider.Stub(
				x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ScheduleDate, personSelectorOrganization1))
				.Return(true);
			permissionProvider.Stub(
				x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ScheduleDate, personSelectorOrganization2))
				.Return(true);

			permissionProvider.Stub(x => x.IsPersonSchedulePublished(data.ScheduleDate, person1)).Return(true);
			permissionProvider.Stub(x => x.IsPersonSchedulePublished(data.ScheduleDate, person2)).Return(true);

			var ret = provider.RetrievePersons(data);
			ret.First().Should().Be.EqualTo(person1.Id.Value);
			ret.Second().Should().Be.EqualTo(person2.Id.Value);
		}
		
		[Test]
		public void ShouldNotRetrievePersonsFromTeamWhenHisScheduleDidntPublished()
		{
			var person1 = PersonFactory.CreatePersonWithId();

			var personSelectorOrganization1 = new PersonSelectorOrganization {PersonId = person1.Id.GetValueOrDefault()};

			personForShiftTradeRepository.Stub(
				x => x.GetPersonForShiftTrade(data.ScheduleDate, data.TeamIdList, null))
				.Return(new IAuthorizeOrganisationDetail[]
				{
					personSelectorOrganization1
				});

			personRepository.Stub(
				x => x.FindPeople(new []{personSelectorOrganization1.PersonId}))
				.Return(new []{person1});

			permissionProvider.Stub(
				x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ScheduleDate, personSelectorOrganization1))
				.Return(true);

			permissionProvider.Stub(x => x.IsPersonSchedulePublished(data.ScheduleDate, person1)).Return(false);

			var ret = provider.RetrievePersons(data);
			ret.Should().Be.Empty();
		}
	}
}
