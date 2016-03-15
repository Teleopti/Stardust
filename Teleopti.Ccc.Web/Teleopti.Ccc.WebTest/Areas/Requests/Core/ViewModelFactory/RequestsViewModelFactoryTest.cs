using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.ViewModelFactory
{
	[TestFixture, RequestsTest]
	public class RequestsViewModelFactoryTest
	{
		public IRequestsViewModelFactory Target;
		public IPersonRequestRepository PersonRequestRepository;
		public IPermissionProvider PermissionProvider;
		public IPeopleSearchProvider PeopleSearchProvider;

		[Test]
		public void ShouldGetRequests()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9)
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(3);
		}	
		
		[Test]
		public void ShouldGetRequestsForDay()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 1)
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetRequestsForSecondDay()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 2),
				EndDate = new DateOnly(2015, 10, 2)
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldOrderByNameCorrectly()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				SortingOrders = new List<RequestsSortingOrder> { RequestsSortingOrder.AgentNameAsc }
			};
		
			var result = Target.Create(input);
			result.First().AgentName.Should().Be.EqualTo("test1 test1");
			result.Second().AgentName.Should().Be.EqualTo("test2 test2");
			result.Last().AgentName.Should().Be.EqualTo("test3 test3");

		}
		
		[Test]
		public void ShouldNotSeeRequestWithoutPermission()
		{
			setUpRequests();

			var permissionProvider = PermissionProvider as Global.FakePermissionProvider;
			permissionProvider.Enable();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldNotSeeRequestBeforePermissionDate()
		{
			
			setUpRequests();

			var permissionProvider = PermissionProvider as Global.FakePermissionProvider;
			permissionProvider.Enable();

			permissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebRequests, new DateOnly(2015, 10, 3));

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31)
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(1);
		}

		
		[Test]
		public void ShouldReturnRequestsBelongToQueriedAgents()
		{
			
			var personSearchProvider = PeopleSearchProvider as FakePeopleSearchProvider;
			var requests = setUpRequests();

		
			personSearchProvider.PresetReturnPeople(new List<IPerson>{requests.First().Person} );

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string> { {PersonFinderField.FirstName, "test1"}}					
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(1);
			result.First().AgentName.Should().Be.EqualTo("test1 test1");

		}

		[Test]
		public void ShouldGetRequestsInRequestListViewModel()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9)
			};

			var result = Target.CreateRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(3);
			result.TotalCount.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldNotSeeRequestBeforePermissionDateInRequestListViewModel()
		{

			setUpRequests();

			var permissionProvider = PermissionProvider as Global.FakePermissionProvider;
			permissionProvider.Enable();

			permissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebRequests, new DateOnly(2015, 10, 3));

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31)
			};

			var result = Target.CreateRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnRequestsBelongToQueriedAgentsInRequestListViewModel()
		{

			var personSearchProvider = PeopleSearchProvider as FakePeopleSearchProvider;
			var requests = setUpRequests();


			personSearchProvider.PresetReturnPeople(new List<IPerson> { requests.First().Person });

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string> { { PersonFinderField.FirstName, "test1" } }
			};

			var result = Target.CreateRequestListViewModel(input);
			result.TotalCount.Should().Be.EqualTo(1);
			result.Requests.First().AgentName.Should().Be.EqualTo("test1 test1");

		}


		private IEnumerable<IPersonRequest> setUpRequests()
		{
			var textRequest1 = new TextRequest(new DateTimePeriod(2015, 10, 1, 2015, 10, 6));
			var absenceRequest = new AbsenceRequest(AbsenceFactory.CreateAbsence("absence1"), new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var textRequest2 = new TextRequest(new DateTimePeriod(2015, 10, 2, 2015, 10, 7));


			var personRequest1 = new PersonRequest(PersonFactory.CreatePerson("test1"), textRequest1);
			personRequest1.SetId(Guid.NewGuid());
			var personRequest2 = new PersonRequest(PersonFactory.CreatePerson("test2"), absenceRequest);
			personRequest2.SetId(Guid.NewGuid());
			var personRequest3 = new PersonRequest(PersonFactory.CreatePerson("test3"), textRequest2);
			personRequest3.SetId(Guid.NewGuid());

			var personRequestRepository = PersonRequestRepository as FakePersonRequestRepository;
			personRequestRepository.Add(personRequest1);
			personRequestRepository.Add(personRequest2);
			personRequestRepository.Add(personRequest3);

			return new List<IPersonRequest> { personRequest1, personRequest2, personRequest3 };
		}

	}
}