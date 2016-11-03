﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture, MyTimeWebTest]
	public class PersonRequestProviderTest
	{
		public IPersonRequestProvider RequestProvider;
		public IPersonRequestRepository RequestRepository;
		public IPermissionProvider PermissionProvider;
		public ILoggedOnUser LoggedOnUser;

		[Test]
		public void ShouldRetrieveRequestsForCurrentUserAndDays()
		{
			var repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			var person = new Person();
			var target = new PersonRequestProvider(repository, loggedOnUser, userTimeZone, new FakePermissionProvider());
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(3));
			var personRequests = new IPersonRequest[] { };

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			userTimeZone.Stub(x => x.TimeZone()).Return((TimeZoneInfo.Local));
			repository.Stub(x => x.FindAllRequestsForAgent(person, period.ToDateTimePeriod(userTimeZone.TimeZone()))).Return(personRequests);

			var result = target.RetrieveRequestsForLoggedOnUser(period);

			result.Should().Be.SameInstanceAs(personRequests);
		}

		[Test]
		public void ShouldRetrieveRequestById()
		{
			var repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var target = new PersonRequestProvider(repository, null, null, new FakePermissionProvider());
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
			var target = new PersonRequestProvider(repository, null, null, new FakePermissionProvider());
			var id = Guid.NewGuid();

			repository.Stub(rep => rep.Get(id)).Return(null);

			Assert.Throws<DataSourceException>(() =>
											   target.RetrieveRequest(id));
		}

		[Test]
		public void ShouldFindAllRequestsForCurrentUserWithPaging()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var target = new PersonRequestProvider(repository, loggedOnUser, null, new FakePermissionProvider());
			var person = new Person();
			var paging = new Paging();
			var filter = new RequestListFilter() { HideOldRequest = false, SortByUpdateDate = true };
			var personRequests = new[]
			{
				new PersonRequestFactory().CreatePersonRequest(),
				new PersonRequestFactory().CreatePersonRequest()
			};
			var requestTypes = new List<RequestType>
			{
				RequestType.AbsenceRequest,RequestType.TextRequest
			};
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			repository.Stub(x => x.FindAllRequestsForAgentByType(person, paging, null, requestTypes.ToArray())).Return(personRequests);

			Assert.That(personRequests.Length, Is.EqualTo(target.RetrieveRequestsForLoggedOnUser(paging, filter).Count()));
			repository.AssertWasCalled(x => x.FindAllRequestsForAgentByType(person, paging, null, requestTypes.ToArray()));
		}

		[Test]
		public void ShouldFindAllRequestsForCurrentUserAfterSpecificDateWithPaging()
		{
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
			var person = MockRepository.GenerateMock<IPerson>();
			person.Stub(x => x.PermissionInformation.DefaultTimeZone()).Return(timezone);

			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var target = new PersonRequestProvider(repository, loggedOnUser, null, new FakePermissionProvider());
			var paging = new Paging();

			var personRequestFactory = new PersonRequestFactory
			{
				Person = person
			};

			var personRequests = new[]
			{
				personRequestFactory.CreatePersonRequest(),
				personRequestFactory.CreatePersonRequest()
			};
			var requestTypes = new List<RequestType>
			{
				RequestType.AbsenceRequest,
				RequestType.TextRequest
			};

			var earliestDateLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone).AddDays(-10).Date;
			var earliestDateUtc = TimeZoneInfo.ConvertTimeToUtc(earliestDateLocal, timezone);
			var filter = new RequestListFilter() { HideOldRequest = true, SortByUpdateDate = true };
			repository.Stub(x => x.FindAllRequestsForAgentByType(person, paging, earliestDateUtc, requestTypes.ToArray()))
				.IgnoreArguments().Return(personRequests);

			var result = target.RetrieveRequestsForLoggedOnUser(paging, filter);
			repository.AssertWasCalled(x => x.FindAllRequestsForAgentByType(
				Arg<IPerson>.Matches(p => p == person),
				Arg<Paging>.Matches(p => p.Equals(paging)),
				Arg<DateTime>.Matches(d => d == earliestDateUtc),
				Arg<RequestType[]>.Matches(r => r.SequenceEqual(requestTypes))));
			Assert.That(personRequests.Length, Is.EqualTo(result.Count()));
		}

		[Test]
		public void ShouldFindAllRequestsExceptOfferWithPaging()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var target = new PersonRequestProvider(repository, loggedOnUser, null, new FakePermissionProvider());
			var person = new Person();
			var paging = new Paging{ Skip = 0, Take = 5 };
			var filter = new RequestListFilter() { HideOldRequest = false, SortByUpdateDate = true };
			var personRequests = new[]
			{
				new PersonRequestFactory().CreatePersonRequest(),
				new PersonRequestFactory().CreatePersonRequest()
			};
			var requestTypes = new List<RequestType>
			{
				RequestType.AbsenceRequest,RequestType.TextRequest
			};
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			repository.Stub(x => x.FindAllRequestsForAgentByType(person, paging, null, requestTypes.ToArray())).Return(personRequests);

			Assert.That(personRequests.Length, Is.EqualTo(target.RetrieveRequestsForLoggedOnUser(paging, filter).Count()));
			repository.AssertWasCalled(x => x.FindAllRequestsForAgentByType(person, paging, null, requestTypes.ToArray()));
		}

		[Test]
		public void ShouldNotReturnShiftTradeRequestWithoutShiftTradePermission()
		{
			var person = PersonFactory.CreatePerson("Bill");
			var requestDate = new DateOnly(2015, 7, 21);
			var shiftTradeRequest = new PersonRequestFactory().CreatePersonShiftTradeRequest(person, requestDate);
			var filter = new RequestListFilter() {HideOldRequest = false, SortByUpdateDate = true};

			((FakeLoggedOnUser)LoggedOnUser).SetFakeLoggedOnUser(person);
			RequestRepository.Add(shiftTradeRequest);

			var requestQueue = RequestProvider.RetrieveRequestsForLoggedOnUser(new Paging {Skip = 0, Take = 5}, filter).ToArray();

			requestQueue.Length.Should().Be(0);
			Assert.IsFalse(requestQueue.Any(requestFromProvider =>
					requestFromProvider.Request.RequestType == RequestType.ShiftTradeRequest));
		}

		[Test]
		public void ShouldReturnShiftTradeRequestIfHasShiftTradePermission()
		{
			var person = PersonFactory.CreatePerson("John");
			var requestDate = DateOnly.Today;
			var shiftTradeRequest = new PersonRequestFactory().CreatePersonShiftTradeRequest(person, requestDate);
			var filter = new RequestListFilter() { HideOldRequest = false, SortByUpdateDate = true };

			((FakeLoggedOnUser)LoggedOnUser).SetFakeLoggedOnUser(person);
			RequestRepository.Add(shiftTradeRequest);
			((FakePermissionProvider)PermissionProvider).PermitPerson(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, requestDate, person);

			var requestQueue = RequestProvider.RetrieveRequestsForLoggedOnUser(new Paging { Skip = 0, Take = 5 }, filter).ToArray();

			requestQueue.Length.Should().Be(1);
			Assert.IsTrue(requestQueue.Count(requestFromProvider =>
				requestFromProvider.Request.RequestType == RequestType.ShiftTradeRequest) == 1);
		}

		[Test]
		public void ShouldReturnTextRequestWithoutShiftTradePermission()
		{
			var person = PersonFactory.CreatePerson("John");
			var requestDate = DateOnly.Today;
			var shiftTradeRequest = new PersonRequestFactory().CreatePersonShiftTradeRequest(person, requestDate);
			var textRequest = new PersonRequestFactory().CreatePersonRequest(person);
			var filter = new RequestListFilter() { HideOldRequest = false, SortByUpdateDate = true };

			((FakeLoggedOnUser)LoggedOnUser).SetFakeLoggedOnUser(person);
			RequestRepository.Add(shiftTradeRequest);
			RequestRepository.Add(textRequest);

			var requestQueue = RequestProvider.RetrieveRequestsForLoggedOnUser(new Paging {Skip = 0, Take = 5}, filter).ToArray();

			requestQueue.Length.Should().Be(1);
			Assert.IsTrue(requestQueue.Any(requestFromProvider =>
				requestFromProvider.Request.RequestType == RequestType.TextRequest));
		}

		[Test]
		public void ShouldReturnCorrectAmounteOfRequestIfHasPermission()
		{
			var person = PersonFactory.CreatePerson("John");
			var requestDate = DateOnly.Today;
			var shiftTradeRequest = new PersonRequestFactory().CreatePersonShiftTradeRequest(person, requestDate);
			var textRequest = new PersonRequestFactory().CreatePersonRequest(person);
			var absence = AbsenceFactory.CreateAbsence("Sick leave");
			var filter = new RequestListFilter() { HideOldRequest = false, SortByUpdateDate = true };
			var period = new DateTimePeriod(
				new DateTime(2008, 7, 10, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 7, 11, 0, 0, 0, DateTimeKind.Utc));
			IPersonRequest request = new PersonRequest(person);
			IAbsenceRequest absenceRequest = new AbsenceRequest(absence, period);
			request.Request = absenceRequest;

			((FakeLoggedOnUser)LoggedOnUser).SetFakeLoggedOnUser(person);
			RequestRepository.Add(shiftTradeRequest);
			RequestRepository.Add(textRequest);
			RequestRepository.Add(request);

			var requestQueue = RequestProvider.RetrieveRequestsForLoggedOnUser(new Paging { Skip = 0, Take = 5 }, filter).ToArray();

			requestQueue.Length.Should().Be(2);
			Assert.IsTrue(requestQueue.Any(requestFromProvider =>
				requestFromProvider.Request.RequestType == RequestType.TextRequest || requestFromProvider.Request.RequestType == RequestType.ShiftTradeRequest));
		}
	}
}