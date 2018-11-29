using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class SchedulePersonProviderTest
	{
		[Test]
		public void ShouldGetPersonsInGivenGroup()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var groupingReadOnlyRepository = MockRepository.GenerateMock<IGroupingReadOnlyRepository>();
			var groupId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var readOnlyGroupDetail = new ReadOnlyGroupDetail
				{
					PersonId = personId
				};

			groupingReadOnlyRepository.Stub(x => x.DetailsForGroup(groupId, DateOnly.Today)).Return(new[] {readOnlyGroupDetail});

			var target = new SchedulePersonProvider(personRepository, new FakePermissionProvider(), groupingReadOnlyRepository);

			target.GetPermittedPersonsForGroup(DateOnly.Today, groupId, DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			personRepository.AssertWasCalled(x => x.FindPeople(Arg<IEnumerable<Guid>>.List.ContainsAll(new[]{personId})));
		}


		[Test]
		public void ShouldFilterPermittedPersonWhenQueryingGivenPeopleList()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var persons = new[] { new Person(), new Person() };

			personRepository.Stub(x => x.FindPeople(persons.Select(p=>p.Id.GetValueOrDefault()))).IgnoreArguments().Return(persons);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, persons.ElementAt(0))).Return(false);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, persons.ElementAt(1))).Return(true);

			var target = new SchedulePersonProvider(personRepository, permissionProvider, null);

			var result = target.GetPermittedPeople(new GroupScheduleInput { PersonIds = persons.Select(p => p.Id.GetValueOrDefault()), ScheduleDate = DateOnly.Today.Date}, DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			result.Single().Should().Be(persons.ElementAt(1));
		}
	}
}