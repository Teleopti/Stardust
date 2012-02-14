using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class PersonRequestProviderTest
	{
		[Test]
		public void ShouldFindTextRequestsForCurrentUserWithPaging()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var target = new PersonRequestProvider(repository, loggedOnUser, null);
			var person = new Person();
			var paging = new Paging();
			var personRequests = new IPersonRequest[] { };

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			repository.Stub(x => x.FindTextRequestsForAgent(person, paging)).Return(personRequests);

			target.RetrieveTextRequests(paging);

			repository.AssertWasCalled(x => x.FindTextRequestsForAgent(person, paging));
		}

		[Test]
		public void ShouldRetrieveRequestsForCurrentUserAndDays()
		{
			var repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			var person = new Person();
			var target = new PersonRequestProvider(repository, loggedOnUser, userTimeZone);
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(3));
			var personRequests = new IPersonRequest[] {};

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			userTimeZone.Stub(x => x.TimeZone()).Return(new CccTimeZoneInfo(TimeZoneInfo.Local));
			repository.Stub(x => x.FindAllRequestsForAgent(person, period.ToDateTimePeriod(userTimeZone.TimeZone()))).Return(personRequests);

			var result = target.RetrieveRequests(period);

			result.Should().Be.SameInstanceAs(personRequests);
		}

		[Test]
		public void ShouldRetrieveRequestById()
		{
			var repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var target = new PersonRequestProvider(repository, null, null);
			var id = Guid.NewGuid();
			var personRequests = new PersonRequest(new Person());
			personRequests.SetId(id);

			repository.Stub(rep => rep.Get(id)).Return(personRequests);

			var result = target.RetrieveRequest(id);

			result.Should().Be.EqualTo(personRequests);
		}

		[Test]
		public void ShouldThrowIfNotExistInDataSource()
		{
			var repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var target = new PersonRequestProvider(repository, null, null);
			var id = Guid.NewGuid();

			repository.Stub(rep => rep.Get(id)).Return(null);

			Assert.Throws<DataSourceException>(() =>
			                                   target.RetrieveRequest(id));
		}
	}
}
