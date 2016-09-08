using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	///<summary>
	/// Tests PersonRequestRepository
	///</summary>
	[TestFixture]
	[Category("LongRunning")]
	public class PersonRequestRepositoryTest : RepositoryTest<IPersonRequest>
	{
		private IPerson _person;
		private IAbsence _absence;
		private IScenario _defaultScenario;
		private Team _team;
		private Site _site;
		private Contract _contract;
		private PartTimePercentage _partTimePercentage;
		private IContractSchedule _contractSchedule;

		/// <summary>
		/// Runs every test. Implemented by repository's concrete implementation.
		/// </summary>
		protected override void ConcreteSetup()
		{
			_defaultScenario = ScenarioFactory.CreateScenarioAggregate("Default", true);
			_person = PersonFactory.CreatePerson("sdfoj");
			_absence = AbsenceFactory.CreateAbsence("Sick leave");

			_team = TeamFactory.CreateSimpleTeam("team");
			_site = SiteFactory.CreateSimpleSite("site");
			_team.Site = _site;
			_contract = new Contract("contract");
			_partTimePercentage = new PartTimePercentage("partTimePercentage");
			_contractSchedule = ContractScheduleFactory.CreateContractSchedule("contractSchedule");

			PersistAndRemoveFromUnitOfWork(_site);
			PersistAndRemoveFromUnitOfWork(_team);
			PersistAndRemoveFromUnitOfWork(_contract);
			PersistAndRemoveFromUnitOfWork(_partTimePercentage);
			PersistAndRemoveFromUnitOfWork(_contractSchedule);

			PersistAndRemoveFromUnitOfWork(_defaultScenario);
			PersistAndRemoveFromUnitOfWork(_person);
			PersistAndRemoveFromUnitOfWork(_absence);
		}

		/// <summary>
		/// Creates an aggregate using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		protected override IPersonRequest CreateAggregateWithCorrectBusinessUnit()
		{
			return createAbsenceRequestAndBusinessUnit();
		}

		private IPersonRequest createAbsenceRequestAndBusinessUnit(IAbsence absence = null)
		{
			var period = new DateTimePeriod(
				new DateTime(2008, 7, 10, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 7, 11, 0, 0, 0, DateTimeKind.Utc));
			IPersonRequest request = new PersonRequest(_person);
			IAbsenceRequest absenceRequest = new AbsenceRequest(absence ?? _absence, period);

			request.Request = absenceRequest;
			request.Pending();

			return request;
		}

	
		private IPersonRequest createShiftExchangeOffer(DateTime startDate, DateTime? shiftDate = null,
			DateTime? validTo = null)
		{
			var shiftDateOnly = shiftDate != null ? new DateOnly(shiftDate.Value) : new DateOnly(2008, 5, 1);
			var currentShift = ScheduleDayFactory.Create(shiftDateOnly, _person);

			var dayFilterCriteria = new ScheduleDayFilterCriteria(ShiftExchangeLookingForDay.WorkingShift,
				new DateTimePeriod(startDate, startDate.AddDays(1)));

			var validToDateOnly = validTo != null ? new DateOnly(validTo.Value) : new DateOnly(2008, 7, 9);
			var offer = new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(validToDateOnly, dayFilterCriteria),
				ShiftExchangeOfferStatus.Pending);

			var request = new PersonRequest(_person)
			{
				Request = offer
			};
			request.Pending();

			return request;
		}

		/// <summary>
		/// Creates an aggregate using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		protected IPersonRequest CreateShiftTradeRequest(string tradeWithName)
		{
			IPersonRequest request = new PersonRequest(_person);
			IPerson tradeWithPerson = PersonFactory.CreatePerson(tradeWithName);
			PersistAndRemoveFromUnitOfWork(tradeWithPerson);
			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(_person, tradeWithPerson, new DateOnly(2008, 7, 16),
						new DateOnly(2008, 7, 16)),
					new ShiftTradeSwapDetail(_person, tradeWithPerson, new DateOnly(2008, 7, 17),
						new DateOnly(2008, 7, 17)),
					new ShiftTradeSwapDetail(_person, tradeWithPerson, new DateOnly(2008, 7, 18),
						new DateOnly(2008, 7, 18)),
					new ShiftTradeSwapDetail(_person, tradeWithPerson, new DateOnly(2008, 7, 19),
						new DateOnly(2008, 7, 19))
				});
			foreach (var shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
			{
				shiftTradeSwapDetail.ChecksumFrom = 50;
				shiftTradeSwapDetail.ChecksumTo = 57;
			}
			request.Request = shiftTradeRequest;
			request.Pending();

			return request;
		}

		/// <summary>
		/// Verifies the aggregate graph properties.
		/// </summary>
		/// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
		protected override void VerifyAggregateGraphProperties(IPersonRequest loadedAggregateFromDatabase)
		{
			IPersonRequest org = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(org.Person, loadedAggregateFromDatabase.Person);
			Assert.AreEqual(((IAbsenceRequest) org.Request).Absence,
				((IAbsenceRequest) loadedAggregateFromDatabase.Request).Absence);
			Assert.AreEqual((org.Request).Period,
				(loadedAggregateFromDatabase.Request).Period);
		}

		protected override Repository<IPersonRequest> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PersonRequestRepository(currentUnitOfWork);
		}

		private void setUpGetRequestsByTypeTests()
		{
			var personFrom = PersonFactory.CreatePerson("personFrom");

			PersistAndRemoveFromUnitOfWork(personFrom);
			var absence = new Absence()
			{
				Description = new Description("test absence")
			};
			PersistAndRemoveFromUnitOfWork(absence);

			var requestDate = DateOnly.Today;
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				new ShiftTradeSwapDetail(personFrom, _person, requestDate, requestDate)
			});
			var shiftTradePersonRequest = new PersonRequest(personFrom) {Request = shiftTradeRequest};
			var textRequest = new PersonRequest(_person, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
			var absenceRequest = new PersonRequest(_person,
				new AbsenceRequest(absence, new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
			var offerRequest = createShiftExchangeOffer(new DateTime(2008, 4, 1, 0, 0, 0, DateTimeKind.Utc));

			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);
			PersistAndRemoveFromUnitOfWork(textRequest);
			PersistAndRemoveFromUnitOfWork(absenceRequest);
			PersistAndRemoveFromUnitOfWork(offerRequest);
		}

		[Test]
		public void FindNonExistingShouldReturnNull()
		{
			new PersonRequestRepository(UnitOfWork).Find(Guid.NewGuid())
				.Should().Be.Null();
		}

		[Test]
		public void CanCreateShiftExchangeOffer()
		{
			var startDate = new DateTime(2008, 4, 1, 0, 0, 0, DateTimeKind.Utc);

			IPersonRequest offerRequest = createShiftExchangeOffer(startDate);
			PersistAndRemoveFromUnitOfWork(offerRequest);

			DateTimePeriod period = new DateTimePeriod(2008, 04, 1, 2008, 07, 20);
			IList<IPersonRequest> foundRequests = new PersonRequestRepository(UnitOfWork).Find(_person, period);
			Assert.AreEqual(1, foundRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundRequests[0].Request));
			Assert.IsTrue(foundRequests.Contains(offerRequest));
		}

		[Test]
		public void FindPendingPersonRequestByRequestTypeAndStartDate()
		{
			var startDate = new DateTime(2008, 4, 1, 0, 0, 0, DateTimeKind.Utc);
			var startDate2 = startDate.AddDays(1);

			var shiftExchangeOfferPersonRequest = createShiftExchangeOffer(startDate);
			var shiftExchangeOfferPersonRequest2 = createShiftExchangeOffer(startDate2);

			shiftExchangeOfferPersonRequest2.Deny(null, "bla", new PersonRequestCheckAuthorization());

			PersistAndRemoveFromUnitOfWork(shiftExchangeOfferPersonRequest);
			PersistAndRemoveFromUnitOfWork(shiftExchangeOfferPersonRequest2);

			var foundShiftExchangeRequests = new PersonRequestRepository(UnitOfWork).FindByStatus<ShiftExchangeOffer>(_person,
				startDate, 0);

			Assert.AreEqual(1, foundShiftExchangeRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundShiftExchangeRequests[0].Request));
			Assert.IsTrue(foundShiftExchangeRequests.Contains(shiftExchangeOfferPersonRequest));
		}

		[Test]
		public void VerifyCanFindRequestsForPeriodForPerson()
		{
			IPersonRequest requestAccepted = CreateShiftTradeRequest("Trade with me");
			IPersonRequest requestAbsence = CreateAggregateWithCorrectBusinessUnit();

			PersistAndRemoveFromUnitOfWork(requestAccepted);
			PersistAndRemoveFromUnitOfWork(requestAbsence);

			DateTimePeriod period = new DateTimePeriod(2008, 07, 15, 2008, 07, 20);
			IList<IPersonRequest> foundRequests = new PersonRequestRepository(UnitOfWork).Find(_person, period);

			Assert.AreEqual(1, foundRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundRequests[0].Request));
			Assert.IsTrue(foundRequests.Contains(requestAccepted));
		}

		[Test]
		public void ShouldFindShiftTradeRequestUpdateAfter()
		{
			var shiftTradeRequest = CreateShiftTradeRequest("Trade with me");
			PersistAndRemoveFromUnitOfWork(shiftTradeRequest);
			var foundRequest = new PersonRequestRepository(UnitOfWork).FindPersonRequestUpdatedAfter(DateTime.UtcNow.AddHours(-1));
			foundRequest.Count.Should().Be.EqualTo(1);
			foundRequest.First().Id.Value.Should().Be.EqualTo(shiftTradeRequest.Id.Value);
		}

		[Test]
		public void ShouldNotFindShiftTradeRequestUpdateAfter()
		{
			var shiftTradeRequest = CreateShiftTradeRequest("Trade with me");
			PersistAndRemoveFromUnitOfWork(shiftTradeRequest);
			var foundRequest = new PersonRequestRepository(UnitOfWork).FindPersonRequestUpdatedAfter(DateTime.UtcNow.AddHours(1));
			foundRequest.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindOtherRequestUpdatedAfter()
		{
			var requestAbsence = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(requestAbsence);
			var foundRequest = new PersonRequestRepository(UnitOfWork).FindPersonRequestUpdatedAfter(DateTime.UtcNow.AddHours(-1));
			foundRequest.Count.Should().Be.EqualTo(1);
			foundRequest.First().Id.GetValueOrDefault().Should().Be.EqualTo(requestAbsence.Id.Value);
		}

		[Test]
		public void ShouldNotFindOtherRequestUpdatedAfter()
		{
			var requestAbsence = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(requestAbsence);
			var foundRequest = new PersonRequestRepository(UnitOfWork).FindPersonRequestUpdatedAfter(DateTime.UtcNow.AddHours(1));
			foundRequest.Should().Be.Empty();
		}

		[Test]
		public void VerifyCanFindRequestWithCertainGuid()
		{
			IPersonRequest requestAccepted = CreateShiftTradeRequest("Trade with me");
			IPersonRequest requestAbsence = CreateAggregateWithCorrectBusinessUnit();

			PersistAndRemoveFromUnitOfWork(requestAccepted);
			PersistAndRemoveFromUnitOfWork(requestAbsence);

			Assert.IsNotNull(requestAccepted.Id);
			Guid? savedGuid = requestAccepted.Id;

			IPersonRequest loadedPersonRequest = new PersonRequestRepository(UnitOfWork).Find((Guid) savedGuid);

			Assert.IsNotNull(loadedPersonRequest);
		}

		[Test]
		public void VerifyCanFindRequestWithCertainGuids()
		{
			IPersonRequest requestAccepted = CreateShiftTradeRequest("Trade with me");
			IPersonRequest requestAbsence = CreateAggregateWithCorrectBusinessUnit();

			PersistAndRemoveFromUnitOfWork(requestAccepted);
			PersistAndRemoveFromUnitOfWork(requestAbsence);

			Assert.IsNotNull(requestAccepted.Id);
			Guid acceptedId = requestAccepted.Id.GetValueOrDefault();
			Guid absenceId = requestAbsence.Id.GetValueOrDefault();

			var requestIds = new List<Guid> {acceptedId, absenceId};

			var loadedPersonRequest = new PersonRequestRepository(UnitOfWork).Find(requestIds);

			loadedPersonRequest.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldFindAllRequestsForAgent()
		{
			IPersonRequest personRequestWithAbsenceRequest = CreateAggregateWithCorrectBusinessUnit();

			IPerson personFrom = PersonFactory.CreatePerson("vjiosd");
			personFrom.Name = new Name("mala", "mala");
			PersistAndRemoveFromUnitOfWork(personFrom);

			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 16),
						new DateOnly(2008, 7, 16)),
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 17),
						new DateOnly(2008, 7, 17)),
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 18),
						new DateOnly(2008, 7, 18)),
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 19),
						new DateOnly(2008, 7, 19))
				});

			IPersonRequest personRequestWithShiftTrade = new PersonRequest(personFrom);
			personRequestWithShiftTrade.Request = shiftTradeRequest;

			PersistAndRemoveFromUnitOfWork(personRequestWithAbsenceRequest);
			PersistAndRemoveFromUnitOfWork(personRequestWithShiftTrade);

			var foundRequests = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgent(_person);

			int actualValue = foundRequests.Count();
			Assert.AreEqual(2, actualValue);
		}

		[Test]
		public void VerifyFindAllRequestsForAgentShouldExcludeAutoDeniedForRecipient()
		{
			IPersonRequest personRequestWithAbsenceRequest = CreateAggregateWithCorrectBusinessUnit();

			IPerson personFrom = PersonFactory.CreatePerson("vjiosd");
			personFrom.Name = new Name("mala", "mala");
			PersistAndRemoveFromUnitOfWork(personFrom);

			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 16),
						new DateOnly(2008, 7, 16)),
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 17),
						new DateOnly(2008, 7, 17)),
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 18),
						new DateOnly(2008, 7, 18)),
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 19),
						new DateOnly(2008, 7, 19))
				});

			IPersonRequest personRequestWithShiftTrade = new PersonRequest(personFrom);
			personRequestWithShiftTrade.Deny(personFrom, string.Empty, new PersonRequestAuthorizationCheckerForTest());
			Assert.IsTrue(personRequestWithShiftTrade.IsAutoDenied); //!!!

			personRequestWithShiftTrade.Request = shiftTradeRequest;

			PersistAndRemoveFromUnitOfWork(personRequestWithAbsenceRequest);
			PersistAndRemoveFromUnitOfWork(personRequestWithShiftTrade);

			var foundRequests = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgent(_person, new Paging {Take = 10});

			int actualValue = foundRequests.Count();
			Assert.AreEqual(1, actualValue);
		}

		[Test]
		public void ShouldFindAllRequestsForAgentWithPagingSinglePage()
		{
			IPersonRequest personRequestWithAbsenceRequest1 = CreateAggregateWithCorrectBusinessUnit();
			IPersonRequest personRequestWithAbsenceRequest2 = CreateAggregateWithCorrectBusinessUnit();

			PersistAndRemoveFromUnitOfWork(personRequestWithAbsenceRequest1);
			PersistAndRemoveFromUnitOfWork(personRequestWithAbsenceRequest2);

			var foundRequests = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgent(_person, new Paging {Take = 1});

			foundRequests.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldFindAllRequestsForAgentWithPagingSecondPageAndSortOnUpdatedOn()
		{
			var personRequestWithAbsenceRequest1 = CreateAggregateWithCorrectBusinessUnit();
			var personRequestWithAbsenceRequest2 = CreateAggregateWithCorrectBusinessUnit();
			var personRequestWithAbsenceRequest3 = CreateAggregateWithCorrectBusinessUnit();

			// ouch! better way to modify updated on?
			PersistAndRemoveFromUnitOfWork(personRequestWithAbsenceRequest1);
			setUpdatedOnForRequest(personRequestWithAbsenceRequest1, -2);
			PersistAndRemoveFromUnitOfWork(personRequestWithAbsenceRequest2);
			setUpdatedOnForRequest(personRequestWithAbsenceRequest2, -1);
			PersistAndRemoveFromUnitOfWork(personRequestWithAbsenceRequest3);

			var results = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgent(_person, new Paging
			{
				Take = 1,
				Skip = 1
			}).ToList();

			results.Should().Have.Count.EqualTo(1);
			results.Single().Should().Be.EqualTo(personRequestWithAbsenceRequest2);
		}

		[Test]
		public void ShouldNotUpdateBrokenRulesAfterFindAllRequestsForAgent()
		{
			IPerson personFrom = PersonFactory.CreatePerson("vjiosd");
			personFrom.Name = new Name("mala", "mala");
			PersistAndRemoveFromUnitOfWork(personFrom);

			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 16),
						new DateOnly(2008, 7, 16)),
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 17),
						new DateOnly(2008, 7, 17)),
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 18),
						new DateOnly(2008, 7, 18)),
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 19),
						new DateOnly(2008, 7, 19))
				});

			IPersonRequest personRequestWithShiftTrade = new PersonRequest(personFrom);
			personRequestWithShiftTrade.Request = shiftTradeRequest;

			PersistAndRemoveFromUnitOfWork(personRequestWithShiftTrade);

			var foundRequests = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgent(_person);
			var foundRequest = foundRequests.FirstOrDefault();
			foundRequest.BrokenBusinessRules.Should(null);
			foundRequest.UpdatedOn.Should().Equals(personRequestWithShiftTrade.UpdatedOn);
		}

		private void setUpdatedOnForRequest(IPersonRequest personRequest, int minutes)
		{
			const string sql = "UPDATE dbo.PersonRequest SET UpdatedOn = DATEADD(mi,:Minutes,UpdatedOn) WHERE Id=:Id;";
			Session.CreateSQLQuery(sql)
				.SetGuid("Id", personRequest.Id.GetValueOrDefault())
				.SetInt32("Minutes", minutes)
				.ExecuteUpdate();
		}

		[Test]
		public void ShouldFindAllRequestsForAgentAndPeriod()
		{
			var personRequestInPeriod =
				new PersonRequest(_person,
					new AbsenceRequest(_absence, new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(3))));
			var personRequestNotInperiod =
				new PersonRequest(_person,
					new AbsenceRequest(_absence, new DateTimePeriod(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2))));

			PersistAndRemoveFromUnitOfWork(personRequestInPeriod);
			PersistAndRemoveFromUnitOfWork(personRequestNotInperiod);

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgent(_person,
				new DateTimePeriod(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1)));

			result.Single().Should().Be(personRequestInPeriod);
		}

		[Test]
		public void VerifyFindAllRequestModifiedWithinPeriodOrPending()
		{
			IPersonRequest personRequest = CreateAggregateWithCorrectBusinessUnit();
			IPersonRequest pendingPersonRequest = CreateAggregateWithCorrectBusinessUnit();

			IPerson personTo = PersonFactory.CreatePerson("vjiosd");
			personTo.Name = new Name("mala", "mala");
			PersistAndRemoveFromUnitOfWork(personTo);

			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16),
						new DateOnly(2008, 7, 16))
				});

			IShiftTradeRequest shiftTradeRequest2 = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16),
						new DateOnly(2008, 7, 16))
				});

			IPersonRequest pendingShiftTradePersonRequest = new PersonRequest(personTo);
			IPersonRequest shiftTradePersonRequest = new PersonRequest(personTo);
			shiftTradePersonRequest.Request = shiftTradeRequest;
			shiftTradePersonRequest.Pending();
			pendingShiftTradePersonRequest.Request = shiftTradeRequest2;
			pendingShiftTradePersonRequest.Pending();
			pendingPersonRequest.Pending();
			//Set the status:
			shiftTradePersonRequest.Deny(personTo, null, new PersonRequestAuthorizationCheckerForTest());
			personRequest.Deny(personTo, null, new PersonRequestAuthorizationCheckerForTest());
			Assert.IsTrue(pendingPersonRequest.IsPending);
			Assert.IsTrue(pendingShiftTradePersonRequest.IsPending);

			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);
			PersistAndRemoveFromUnitOfWork(personRequest);
			PersistAndRemoveFromUnitOfWork(pendingPersonRequest);
			PersistAndRemoveFromUnitOfWork(pendingShiftTradePersonRequest);

			DateTime updatedOn = personRequest.UpdatedOn.GetValueOrDefault();
			DateTimePeriod periodToLookFor = new DateTimePeriod(updatedOn.Subtract(TimeSpan.FromDays(1)), updatedOn.AddDays(1));
			DateTimePeriod periodOutside = new DateTimePeriod(updatedOn.AddDays(2), updatedOn.AddDays(4));

			IList<IPersonRequest> foundRequests =
				new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(_person, periodToLookFor);
			IList<IPersonRequest> requestWithinOutsidePeriod =
				new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(_person, periodOutside);

			int actualValue = foundRequests.Count;
			Assert.AreEqual(4, actualValue);
			Assert.AreEqual(2, requestWithinOutsidePeriod.Count(r => r.IsPending));
			Assert.AreEqual(2, requestWithinOutsidePeriod.Count);

			foundRequests.All(r => LazyLoadingManager.IsInitialized(r.Request)).Should().Be.True();
			requestWithinOutsidePeriod.All(r => LazyLoadingManager.IsInitialized(r.Request)).Should().Be.True();
		}

		[Test]
		public void VerifyFindAllRequestModifiedWithinPeriodOrPendingShouldNotListAutoDeniedForPersonTo()
		{
			IPerson personFrom = PersonFactory.CreatePerson("vjiosd");
			PersistAndRemoveFromUnitOfWork(personFrom);

			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 16),
						new DateOnly(2008, 7, 16))
				});

			IShiftTradeRequest shiftTradeRequest2 = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personFrom, _person, new DateOnly(2008, 7, 16),
						new DateOnly(2008, 7, 16))
				});

			IPersonRequest deniedShiftTradePersonRequest = new PersonRequest(personFrom);
			deniedShiftTradePersonRequest.Request = shiftTradeRequest2;
			deniedShiftTradePersonRequest.Pending();
			deniedShiftTradePersonRequest.Deny(personFrom, null, new PersonRequestAuthorizationCheckerForTest());
			Assert.IsFalse(deniedShiftTradePersonRequest.IsAutoDenied);
			Assert.IsTrue(deniedShiftTradePersonRequest.IsDenied);

			IPersonRequest autoDeniedShiftTradePersonRequest = new PersonRequest(personFrom);
			autoDeniedShiftTradePersonRequest.Request = shiftTradeRequest;
			autoDeniedShiftTradePersonRequest.Deny(personFrom, null, new PersonRequestAuthorizationCheckerForTest());
			Assert.IsTrue(autoDeniedShiftTradePersonRequest.IsAutoDenied);
			Assert.IsTrue(autoDeniedShiftTradePersonRequest.IsDenied);

			PersistAndRemoveFromUnitOfWork(autoDeniedShiftTradePersonRequest);
			PersistAndRemoveFromUnitOfWork(deniedShiftTradePersonRequest);

			DateTime updatedOn = autoDeniedShiftTradePersonRequest.UpdatedOn.GetValueOrDefault();
			DateTimePeriod insidePeriod = new DateTimePeriod(updatedOn.Subtract(TimeSpan.FromDays(1)), updatedOn.AddDays(1));

			IList<IPersonRequest> foundRequests =
				new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(_person, insidePeriod);

			int actualValue = foundRequests.Count;
			Assert.AreEqual(1, actualValue);
			Assert.AreEqual(1, foundRequests.Count(r => r.IsDenied));
			Assert.AreEqual(0, foundRequests.Count(r => r.IsAutoDenied));

			foundRequests.All(r => LazyLoadingManager.IsInitialized(r.Request)).Should().Be.True();
		}

		[Test]
		public void VerifyPersonRequestForDateTimePeriod()
		{
			IPersonRequest personRequest = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(personRequest);

			IEnumerable<IPersonRequest> requestWithinOutsidePeriod =
				new PersonRequestRepository(UnitOfWork).FindPersonRequestWithinPeriod(personRequest.Request.Period);
			var firstResult = requestWithinOutsidePeriod.First();
			LazyLoadingManager.IsInitialized(firstResult.Person).Should().Be.True();
		}

		[Test]
		public void VerifyPersonRequestShouldLiesWithinPeriod()
		{
			IPersonRequest personRequest = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(personRequest);

			var absenceRequest = CreateAggregateWithCorrectBusinessUnit();
			var textRequest = new PersonRequest(_person, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
			var textRequest2 = new PersonRequest(_person, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));

			PersistAndRemoveFromUnitOfWork(absenceRequest);
			PersistAndRemoveFromUnitOfWork(textRequest);
			PersistAndRemoveFromUnitOfWork(textRequest2);

			IPerson personTo = PersonFactory.CreatePerson("vjiosd");
			personTo.Name = new Name("mala", "mala");
			PersistAndRemoveFromUnitOfWork(personTo);

			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16)),
					new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 18), new DateOnly(2008, 7, 18))
				});

			IPersonRequest shiftTradePersonRequest = new PersonRequest(personTo);
			shiftTradePersonRequest.Request = shiftTradeRequest;
			shiftTradePersonRequest.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

			IShiftTradeRequest shiftTradeRequest2 = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 20), new DateOnly(2008, 7, 20)),
					new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 21), new DateOnly(2008, 7, 21))
				});

			IPersonRequest shiftTradePersonRequest2 = new PersonRequest(personTo);
			shiftTradePersonRequest2.Request = shiftTradeRequest2;
			shiftTradePersonRequest2.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest2);

			IEnumerable<IPersonRequest> requestWithinOutsidePeriod =
				new PersonRequestRepository(UnitOfWork).FindPersonRequestWithinPeriod(shiftTradeRequest2.Period);
			Assert.AreEqual(1, requestWithinOutsidePeriod.Count());
		}

		[Test]
		public void VerifyRequestStatusIsNotNew()
		{
			IPersonRequest request = new PersonRequest(_person);
			var period = new DateTimePeriod(new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 7, 19, 0, 0, 0, DateTimeKind.Utc));
			IAbsenceRequest absenceRequest = new AbsenceRequest(_absence, period);

			request.Request = absenceRequest;

			PersistAndRemoveFromUnitOfWork(request);
			var requestWithStatusNew =
				new PersonRequestRepository(UnitOfWork).FindPersonRequestWithinPeriod(request.Request.Period);
			Assert.IsTrue(requestWithStatusNew.IsEmpty());
		}

		[Test]
		public void VerifyShiftTradeRequestForDateTimePeriod()
		{
			IPerson personTo = PersonFactory.CreatePerson("vjiosd");
			personTo.Name = new Name("mala", "mala");
			PersistAndRemoveFromUnitOfWork(personTo);

			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16)),
					new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 18), new DateOnly(2008, 7, 18))
				});

			IPersonRequest shiftTradePersonRequest = new PersonRequest(personTo);
			shiftTradePersonRequest.Request = shiftTradeRequest;
			shiftTradePersonRequest.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

			IEnumerable<IPersonRequest> requestWithinOutsidePeriod =
				new PersonRequestRepository(UnitOfWork).FindPersonRequestWithinPeriod(shiftTradePersonRequest.Request.Period);
			var firstResult = requestWithinOutsidePeriod.Single();
			LazyLoadingManager.IsInitialized(firstResult.Request).Should().Be.True();
			LazyLoadingManager.IsInitialized(((IShiftTradeRequest) firstResult.Request).ShiftTradeSwapDetails).Should().Be.True();
			LazyLoadingManager.IsInitialized(((IShiftTradeRequest) firstResult.Request).ShiftTradeSwapDetails[0])
				.Should().Be.True();
		}

		[Test]
		public void VerifyFindAllRequestsForAgentsForAPeriod()
		{
			var personRequest = CreateAggregateWithCorrectBusinessUnit();
			var pendingPersonRequest = CreateAggregateWithCorrectBusinessUnit();

			var personTo = PersonFactory.CreatePerson("vjiosd");
			personTo.Name = new Name("mala", "mala");
			PersistAndRemoveFromUnitOfWork(personTo);

			var anotherPerson = PersonFactory.CreatePerson("Smet");
			anotherPerson.Name = new Name("Tom", "Jones");
			PersistAndRemoveFromUnitOfWork(anotherPerson);

			var anotherPerson2 = PersonFactory.CreatePerson("Smet...");
			anotherPerson2.Name = new Name("Kirk", "Douglas");
			PersistAndRemoveFromUnitOfWork(anotherPerson2);

			var persons = new List<IPerson> {_person, anotherPerson, anotherPerson2, personTo};

			var shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16))
				});

			var shiftTradeRequest2 = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16))
				});

			IPersonRequest textRequest = new PersonRequest(anotherPerson);
			textRequest.Request =
				new TextRequest(new DateTimePeriod(new DateTime(2008, 7, 17, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2008, 7, 18, 0, 0, 0, DateTimeKind.Utc)));
			textRequest.Pending();

			IPersonRequest textRequestDenied = new PersonRequest(anotherPerson2);
			textRequestDenied.Request =
				new TextRequest(new DateTimePeriod(new DateTime(2008, 7, 17, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2008, 7, 18, 0, 0, 0, DateTimeKind.Utc)));
			textRequestDenied.Deny(null, null, new PersonRequestAuthorizationCheckerForTest());

			IPersonRequest pendingShiftTradePersonRequest = new PersonRequest(personTo);

			IPersonRequest shiftTradePersonRequest = new PersonRequest(personTo);
			shiftTradePersonRequest.Request = shiftTradeRequest;
			shiftTradePersonRequest.Pending();

			pendingShiftTradePersonRequest.Request = shiftTradeRequest2;
			pendingShiftTradePersonRequest.Pending();
			//Set the status:
			shiftTradePersonRequest.Deny(null, null, new PersonRequestAuthorizationCheckerForTest());
			personRequest.Deny(null, null, new PersonRequestAuthorizationCheckerForTest());
			Assert.IsTrue(pendingPersonRequest.IsPending, "Must be pending");
			Assert.IsTrue(pendingShiftTradePersonRequest.IsPending, "Must be pending");

			//Add 6 requests
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest); //Denied
			PersistAndRemoveFromUnitOfWork(personRequest); //Denied
			PersistAndRemoveFromUnitOfWork(pendingPersonRequest);
			PersistAndRemoveFromUnitOfWork(pendingShiftTradePersonRequest);
			PersistAndRemoveFromUnitOfWork(textRequest);
			PersistAndRemoveFromUnitOfWork(textRequestDenied); //Denied

			var updatedOn = personRequest.UpdatedOn.GetValueOrDefault();
			var periodToLookFor = new DateTimePeriod(updatedOn.Subtract(TimeSpan.FromDays(1)), updatedOn.AddDays(1));
			var periodOutside = new DateTimePeriod(updatedOn.AddDays(2), updatedOn.AddDays(4));

			var foundRequests =
				new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons, periodToLookFor);
			var requestWithinOutsidePeriod =
				new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons, periodOutside);

			var actualValue = foundRequests.Count;
			Assert.AreEqual(6, actualValue);
			Assert.AreEqual(3, requestWithinOutsidePeriod.Count(r => r.IsPending));
			Assert.AreEqual(3, requestWithinOutsidePeriod.Count);
		}

		[Test]
		public void ShouldNotGetExceptionWhenCallingFindAllRequestsModifiedWithinPeriodOrPendingWithMoreThan2100InPersonList()
		{
			IPerson person = PersonFactory.CreatePerson("person");
			PersistAndRemoveFromUnitOfWork(person);

			var personList = Enumerable.Range(1, 2200).Select(s => person).ToArray();
			IList<IPersonRequest> personRequestList =
				new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(personList,
					new DateTimePeriod(2010, 1, 1, 2010, 1, 1));

			Assert.IsNotNull(personRequestList);
		}

		[Test]
		public void VerifyLazyLoadingOfShiftTradeDetails()
		{
			IPerson personTo = PersonFactory.CreatePerson("vjiosd");
			personTo.Name = new Name("mala", "mala");
			PersistAndRemoveFromUnitOfWork(personTo);

			IList<IPerson> persons = new List<IPerson> {_person, personTo};

			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16))
				});
			IPersonRequest shiftTradePersonRequest = new PersonRequest(personTo);
			shiftTradePersonRequest.Request = shiftTradeRequest;
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);
			DateTime updatedOn = shiftTradePersonRequest.UpdatedOn.GetValueOrDefault();
			DateTimePeriod periodToLookFor = new DateTimePeriod(updatedOn.Subtract(TimeSpan.FromDays(1)), updatedOn.AddDays(1));

			IList<IPersonRequest> foundRequests =
				new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons, periodToLookFor);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(((IShiftTradeRequest) foundRequests[0].Request).ShiftTradeSwapDetails));
		}

		[Test]
		public void VerifyCanModifyCollectionOfRequestsForAgent()
		{
			IPersonRequest request1 = CreateAggregateWithCorrectBusinessUnit();

			PersistAndRemoveFromUnitOfWork(request1);

			var period = new DateTimePeriod(new DateTime(2008, 8, 16, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 8, 19, 0, 0, 0, DateTimeKind.Utc));
			IAbsenceRequest newAbsenceRequest = new AbsenceRequest(_absence, period);

			request1.Request = newAbsenceRequest;

			PersistAndRemoveFromUnitOfWork(request1);

			Assert.IsNotNull(request1.Request);
		}

		[Test]
		public void VerifyChangingTheRequestObjectWorks()
		{
			PersonRequestRepository rep = new PersonRequestRepository(UnitOfWork);

			IPersonRequest request1 = CreateAggregateWithCorrectBusinessUnit();
			request1.Request = new AbsenceRequest(_absence, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			PersistAndRemoveFromUnitOfWork(_absence);
			PersistAndRemoveFromUnitOfWork(request1);

			var loaded = rep.Get(request1.Id.GetValueOrDefault());
			loaded.Request = null;
			PersistAndRemoveFromUnitOfWork(loaded);

			loaded = rep.Get(request1.Id.GetValueOrDefault());
			Assert.IsNull(loaded.Request);
		}

		[Test]
		public void VerifyGeneratesPushMessage()
		{
			CleanUpAfterTest();
			IPersonRequest request1 = CreateAggregateWithCorrectBusinessUnit();
			request1.Request = new AbsenceRequest(_absence, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			PersistAndRemoveFromUnitOfWork(_absence);
			PersistAndRemoveFromUnitOfWork(request1);
			request1.Deny(null, "ResourceKey", new PersonRequestAuthorizationCheckerForTest());
			UnitOfWork.PersistAll();

			IList list = Session.CreateCriteria(typeof (PushMessageDialogue)).List();
			Assert.AreEqual(1, list.Count);

			Session.Delete("from PersonRequest");
			Session.Delete("from AbsenceRequest");
			Session.Delete("from Absence");
			Session.Delete("from PushMessageDialogue");
			Session.Delete("from PushMessage");
			((IDeleteTag) _person).SetDeleted();
			Session.Update(_person);
			((IDeleteTag) _defaultScenario).SetDeleted();
			Session.Update(_defaultScenario);
			Session.Flush();
		}

		[Test]
		public void ShouldThrowIfDeletingDeniedRequest()
		{
			var rep = new PersonRequestRepository(UnitOfWork);
			var request = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(request);
			request.Deny(_person, "something", new PersonRequestAuthorizationCheckerForTest());

			Assert.Throws<DataSourceException>(() =>
				rep.Remove(request));
		}

		[Test]
		public void ShouldThrowIfDeletingApprovedRequest()
		{
			var rep = new PersonRequestRepository(UnitOfWork);
			var request = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(request);
			request.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());

			Assert.Throws<DataSourceException>(() => rep.Remove(request));
		}

		[Test]
		public void ShouldNotIncludeShiftTradeOfDeletedPersonTo()
		{
			var personTo = PersonFactory.CreatePerson("person to");
			((IDeleteTag) personTo).SetDeleted();
			var personFrom = PersonFactory.CreatePerson("person from");
			PersistAndRemoveFromUnitOfWork(personTo);
			PersistAndRemoveFromUnitOfWork(personFrom);

			var persons = new List<IPerson> {personFrom, personTo};

			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16))
			});
			var shiftTradePersonRequest = new PersonRequest(personFrom) {Request = shiftTradeRequest};
			shiftTradePersonRequest.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

			new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons,
				new DateTimePeriod(2000, 1, 1, 2010, 1, 1))
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotIncludeShiftTradeOfTerminatedPersonTo()
		{
			var personTo = PersonFactory.CreatePerson("person to");
			personTo.TerminatePerson(new DateOnly(1900, 1, 1), MockRepository.GenerateMock<IPersonAccountUpdater>());
			var personFrom = PersonFactory.CreatePerson("person from");
			PersistAndRemoveFromUnitOfWork(personTo);
			PersistAndRemoveFromUnitOfWork(personFrom);

			var persons = new List<IPerson> {personFrom, personTo};

			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16))
			});
			var shiftTradePersonRequest = new PersonRequest(personFrom) {Request = shiftTradeRequest};
			shiftTradePersonRequest.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

			new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons,
				new DateTimePeriod(2000, 1, 1, 2010, 1, 1)).Should().Be.Empty();
		}

		[Test]
		public void ShouldNotIncludeShiftTradeIfOnlyOneOfTheAgentsIsPassedIn()
		{
			var personTo = PersonFactory.CreatePerson("person to");
			var personFrom = PersonFactory.CreatePerson("person from");
			PersistAndRemoveFromUnitOfWork(personTo);
			PersistAndRemoveFromUnitOfWork(personFrom);

			var shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16))
				});
			var shiftTradePersonRequest = new PersonRequest(personFrom) {Request = shiftTradeRequest};
			shiftTradePersonRequest.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

			new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(new[] {personTo},
				new DateTimePeriod(2000, 1, 1, 2010, 1, 1)).Should().Be.Empty();
		}

		[Test]
		public void ShouldNotIncludeShiftTradeOfDeletedPersonFrom()
		{
			var personTo = PersonFactory.CreatePerson("person to");
			var personFrom = PersonFactory.CreatePerson("person from");
			((IDeleteTag) personFrom).SetDeleted();
			PersistAndRemoveFromUnitOfWork(personTo);
			PersistAndRemoveFromUnitOfWork(personFrom);

			var persons = new List<IPerson> {personFrom, personTo};

			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16))
			});
			var shiftTradePersonRequest = new PersonRequest(personFrom) {Request = shiftTradeRequest};
			shiftTradePersonRequest.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

			new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons,
				new DateTimePeriod(2000, 1, 1, 2010, 1, 1)).Should().Be.Empty();
		}

		[Test]
		public void ShouldNotIncludeShiftTradeOfTerminatedPersonFrom()
		{
			var personTo = PersonFactory.CreatePerson("person to");
			var personFrom = PersonFactory.CreatePerson("person from");
			personFrom.TerminatePerson(new DateOnly(1900, 1, 1), MockRepository.GenerateMock<IPersonAccountUpdater>());
			PersistAndRemoveFromUnitOfWork(personTo);
			PersistAndRemoveFromUnitOfWork(personFrom);

			var persons = new List<IPerson> {personFrom, personTo};

			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16))
			});
			var shiftTradePersonRequest = new PersonRequest(personFrom) {Request = shiftTradeRequest};
			shiftTradePersonRequest.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

			new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons,
				new DateTimePeriod(2000, 1, 1, 2010, 1, 1))
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldOnlyGetShiftTradeRequests()
		{
			var paging = new Paging {Skip = 0, Take = 5};
			setUpGetRequestsByTypeTests();

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgentByType(_person, paging, null,
				RequestType.ShiftTradeRequest);
			result.Single().Request.RequestType.Should().Be(RequestType.ShiftTradeRequest);
		}

		[Test]
		public void ShouldOnlyGetTextRequests()
		{
			var paging = new Paging {Skip = 0, Take = 5};
			setUpGetRequestsByTypeTests();

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgentByType(_person, paging, null,
				RequestType.TextRequest);
			result.Single().Request.RequestType.Should().Be(RequestType.TextRequest);
		}

		[Test]
		public void ShouldOnlyGetAbsenceRequests()
		{
			var paging = new Paging {Skip = 0, Take = 5};
			setUpGetRequestsByTypeTests();

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgentByType(_person, paging, null,
				RequestType.AbsenceRequest);
			result.Single().Request.RequestType.Should().Be(RequestType.AbsenceRequest);
		}

		[Test]
		public void ShouldOnlyGetExchangeOfferRequests()
		{
			var paging = new Paging {Skip = 0, Take = 5};
			setUpGetRequestsByTypeTests();

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgentByType(_person, paging, null,
				RequestType.ShiftExchangeOffer);
			result.Single().Request.RequestType.Should().Be(RequestType.ShiftExchangeOffer);
		}

		[Test]
		public void ShouldGetShiftTradeAndShiftExchangeOfferRequests()
		{
			var paging = new Paging {Skip = 0, Take = 5};
			setUpGetRequestsByTypeTests();

			var requestTypes = new[] { RequestType.ShiftExchangeOffer, RequestType.ShiftTradeRequest };
			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgentByType(_person, paging,
				null, requestTypes).ToArray();
			result.Length.Should().Be(2);
			Assert.IsTrue(result.Any(request => request.Request.RequestType == RequestType.ShiftExchangeOffer));
			Assert.IsTrue(result.Any(request => request.Request.RequestType == RequestType.ShiftTradeRequest));
		}

		[Test]
		public void ShouldOnlyGetRequestAfterSpecificDate()
		{
			var paging = new Paging { Skip = 0, Take = 5 };
			setUpGetRequestsByTypeTests();

			var earliestDate = DateTime.UtcNow.AddDays(-10);
			var requestTypes = new[] {RequestType.ShiftExchangeOffer, RequestType.ShiftTradeRequest};
			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgentByType(_person, paging,
				earliestDate, requestTypes).ToArray();
			result.Length.Should().Be(1);
			Assert.IsTrue(result[0].Request.RequestType == RequestType.ShiftTradeRequest);
		}

		[Test]
		public void ShouldReturnAllRequestsInPeriod()
		{
			setUpGetRequestsByTypeTests();

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1))
			};

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequests(filter).ToArray();
			result.Length.Should().Be(3);
		}

		[Test]
		public void ShouldReturnAllRequestsInPeriodWithWhitelistedTypes()
		{
			setUpGetRequestsByTypeTests();

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
				RequestTypes = new List<RequestType> {RequestType.TextRequest, RequestType.AbsenceRequest}
			};

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequests(filter).ToArray();
			result.Length.Should().Be(2);
		}

		[Test]
		public void ShouldReturnRequestsFromTheSpecifiedPersons()
		{
			var person1 = PersonFactory.CreatePerson("person1");
			var person2 = PersonFactory.CreatePerson("person2");

			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);

			var textRequest1 = new PersonRequest(person1, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
			var textRequest2 = new PersonRequest(person2, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));

			PersistAndRemoveFromUnitOfWork(textRequest1);
			PersistAndRemoveFromUnitOfWork(textRequest2);

			var filter = new RequestFilter()
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
				Persons = new List<IPerson> {person1},
				RequestTypes = new List<RequestType> {RequestType.TextRequest, RequestType.AbsenceRequest}
			};

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequests(filter).ToArray();
			result.Length.Should().Be(1);
		}

		[Test]
		public void ShouldReturnShiftTradeRequestsThatIntersectDatePeriod()
		{
			var person1 = PersonFactory.CreatePerson("person1");
			var person2 = PersonFactory.CreatePerson("person2");
			var person3 = PersonFactory.CreatePerson("person3");

			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);

			createShiftTradeRequest(new DateOnly(2016, 10, 2), new DateOnly(2016, 10, 4), person1, person2);
			createShiftTradeRequest(new DateOnly(2016, 10, 1), new DateOnly(2016, 10, 3), person1, person3);
			createShiftTradeRequest(new DateOnly(2016, 10, 4), new DateOnly(2016, 10, 5), person2, person3);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(new DateTime(2016, 10, 2).Utc(), new DateTime(2016, 10, 4).Utc()),
				Persons = new List<IPerson> {person1, person2, person3},
				RequestTypes = new List<RequestType> {RequestType.ShiftTradeRequest}
			};

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequests(filter).ToArray();
			result.Length.Should().Be(3);
		}

		[Test]
		public void ShouldReturnShiftTradeRequestsWherePersonToMatchesFilter()
		{
			var person1 = PersonFactory.CreatePerson("person1");
			var person2 = PersonFactory.CreatePerson("person2");
			var person3 = PersonFactory.CreatePerson("person3");

			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);

			createShiftTradeRequest(new DateOnly(2016, 10, 2), new DateOnly(2016, 10, 4), person1, person2);
			createShiftTradeRequest(new DateOnly(2016, 10, 1), new DateOnly(2016, 10, 3), person1, person3);
			createShiftTradeRequest(new DateOnly(2016, 10, 4), new DateOnly(2016, 10, 5), person2, person3);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(new DateTime(2016, 10, 2).Utc(), new DateTime(2016, 10, 4).Utc()),
				Persons = new List<IPerson> {person2},
				RequestTypes = new List<RequestType> {RequestType.ShiftTradeRequest}
			};

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequests(filter).ToArray();

			result.Length.Should().Be(2);
		}

		private PersonRequest createShiftTradeRequest(DateOnly dateFrom, DateOnly dateTo, IPerson personFrom, IPerson personTo)
		{
			var shiftTradeSwapDetailList = new List<IShiftTradeSwapDetail>();

			var dateOnlyPeriod = new DateOnlyPeriod(dateFrom, dateTo);

			foreach (var day in dateOnlyPeriod.DayCollection())
			{
				shiftTradeSwapDetailList.Add(new ShiftTradeSwapDetail(personFrom, personTo, day, day));
			}

			var shiftTradeRequest = new ShiftTradeRequest(shiftTradeSwapDetailList);

			var personRequest = new PersonRequest(personFrom, shiftTradeRequest);

			PersistAndRemoveFromUnitOfWork(personRequest);

			return personRequest;
		}

		[Test]
		public void ShouldReturnRequestsWithSorting()
		{
			var person1 = PersonFactory.CreatePerson("A");
			var person2 = PersonFactory.CreatePerson("B");
			var person3 = PersonFactory.CreatePerson("C");

			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);

			var textRequest1 = new PersonRequest(person1, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
			var textRequest2 = new PersonRequest(person2, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
			var textRequest3 = new PersonRequest(person3, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));

			PersistAndRemoveFromUnitOfWork(textRequest1);
			PersistAndRemoveFromUnitOfWork(textRequest2);
			PersistAndRemoveFromUnitOfWork(textRequest3);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.AgentNameDesc}
			};

			var resultDesc = new PersonRequestRepository(UnitOfWork).FindAllRequests(filter).ToArray();

			resultDesc.Should().Have.SameSequenceAs(new List<IPersonRequest> {textRequest3, textRequest2, textRequest1});

			filter.SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.AgentNameAsc};

			var resultAsc = new PersonRequestRepository(UnitOfWork).FindAllRequests(filter).ToArray();

			resultAsc.Should().Have.SameSequenceAs(new List<IPersonRequest> {textRequest1, textRequest2, textRequest3});
		}

		[Test]
		public void ShouldReturnRequestsWithSortingByPeriod()
		{
			var person1 = PersonFactory.CreatePerson("A");
			var person2 = PersonFactory.CreatePerson("B");
			var person3 = PersonFactory.CreatePerson("C");

			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);

			var textRequest1 = new PersonRequest(person1,
				new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1))));
			var textRequest2 = new PersonRequest(person2,
				new TextRequest(new DateTimePeriod(DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3))));
			var textRequest3 = new PersonRequest(person3,
				new TextRequest(new DateTimePeriod(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(5))));

			PersistAndRemoveFromUnitOfWork(textRequest1);
			PersistAndRemoveFromUnitOfWork(textRequest2);
			PersistAndRemoveFromUnitOfWork(textRequest3);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(6)),
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.PeriodStartDesc}
			};

			var resultDesc = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter).ToArray();

			resultDesc.Should().Have.SameSequenceAs(new List<IPersonRequest> {textRequest3, textRequest2, textRequest1});
		}

		[Test]
		public void ShouldReturnRequestsInAgentNameOrderAsc()
		{
			var people = new[]
			{
				PersonFactory.CreatePerson("Ab", "A"),
				PersonFactory.CreatePerson("Aa", "A"),
			};

			var resultDesc = testPeopleNameOrder(people, RequestsSortingOrder.AgentNameAsc);

			resultDesc[0].Person.Should().Be(people[1]);
			resultDesc[1].Person.Should().Be(people[0]);
		}

		[Test]
		public void ShouldReturnRequestsInAgentNameOrderDesc()
		{
			var people = new[]
			{
				PersonFactory.CreatePerson("Aa", "A"),
				PersonFactory.CreatePerson("Ab", "A"),
			};

			var resultDesc = testPeopleNameOrder(people, RequestsSortingOrder.AgentNameDesc);

			resultDesc[0].Person.Should().Be(people[1]);
			resultDesc[1].Person.Should().Be(people[0]);
		}

		private IPersonRequest[] testPeopleNameOrder(IReadOnlyList<IPerson> people, RequestsSortingOrder order)
		{
			people.ForEach(PersistAndRemoveFromUnitOfWork);

			var textRequests = new List<IPersonRequest>
			{
				new PersonRequest(people[0], new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)))),
				new PersonRequest(people[1], new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)))),
			};

			textRequests.ForEach(PersistAndRemoveFromUnitOfWork);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(6)),
				SortingOrders = new List<RequestsSortingOrder> {order}
			};

			var resultDesc = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter).ToArray();
			return resultDesc;
		}

		[Test]
		public void ShouldReturnRequestsWithSortingBySubject()
		{
			var person1 = PersonFactory.CreatePerson("A");
			var person2 = PersonFactory.CreatePerson("B");
			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);

			var textRequest1 = new PersonRequest(person1,
				new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1))));
			var textRequest2 = new PersonRequest(person2,
				new TextRequest(new DateTimePeriod(DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3))));

			textRequest1.Subject = "Aardvark";
			textRequest2.Subject = "Zebra";

			PersistAndRemoveFromUnitOfWork(textRequest1);
			PersistAndRemoveFromUnitOfWork(textRequest2);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(6)),
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.SubjectDesc}
			};

			var resultDesc = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter).ToArray();

			resultDesc.Should().Have.SameSequenceAs(new List<IPersonRequest> {textRequest2, textRequest1});
		}

		[Test]
		public void ShouldReturnRequestsWithSortingBySeniorityDesc()
		{
			var resultDesc = runSeniorityTest(RequestsSortingOrder.SeniorityDesc);
			resultDesc[0].Person.Name.FirstName.Should().Be("3");
			resultDesc[1].Person.Name.FirstName.Should().Be("1");
			resultDesc[2].Person.Name.FirstName.Should().Be("2");
		}

		[Test]
		public void ShouldReturnRequestsWithSortingBySeniorityAsc()
		{
			var resultDesc = runSeniorityTest(RequestsSortingOrder.SeniorityAsc);
			resultDesc[0].Person.Name.FirstName.Should().Be("2");
			resultDesc[1].Person.Name.FirstName.Should().Be("1");
			resultDesc[2].Person.Name.FirstName.Should().Be("3");
		}

		private IPersonRequest[] runSeniorityTest(params RequestsSortingOrder[] sortOrder)
		{
			var person1 = createAndPersistPerson("1", DateOnly.Today.AddDays(-180));
			var person2 = createAndPersistPerson("2", DateOnly.Today.AddDays(-90));
			var person3 = createAndPersistPerson("3", DateOnly.Today.AddDays(-181));

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1));

			createAndPersistTextRequest(person1, period);
			createAndPersistTextRequest(person2, period);
			createAndPersistTextRequest(person3, period);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(6)),
				SortingOrders = sortOrder.ToList()
			};

			return new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter).ToArray();
		}

		[Test]
		public void ShouldReturnRequestsWithSortingByTeam()
		{
			var team1 = TeamFactory.CreateSimpleTeam("team1");
			team1.Site = _site;
			var team2 = TeamFactory.CreateSimpleTeam("team2");
			team2.Site = _site;
			var team3 = TeamFactory.CreateSimpleTeam("team3");
			team3.Site = _site;

			PersistAndRemoveFromUnitOfWork(team1);
			PersistAndRemoveFromUnitOfWork(team2);
			PersistAndRemoveFromUnitOfWork(team3);

			var personPeriodStartDate = DateOnly.Today.AddDays(-1);

			var person1 = PersonFactory.CreatePerson("1");
			var person2 = PersonFactory.CreatePerson("2");

			person1.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(personPeriodStartDate,
				PersonContractFactory.CreatePersonContract(_contract, _partTimePercentage, _contractSchedule), team2));

			person2.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(personPeriodStartDate.AddDays(-100),
				PersonContractFactory.CreatePersonContract(_contract, _partTimePercentage, _contractSchedule), team1));

			person2.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(personPeriodStartDate,
				PersonContractFactory.CreatePersonContract(_contract, _partTimePercentage, _contractSchedule), team3));

			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);

			var textRequest1 = new PersonRequest(person1,
				new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1))));
			var textRequest2 = new PersonRequest(person2,
				new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1))));

			PersistAndRemoveFromUnitOfWork(textRequest1);
			PersistAndRemoveFromUnitOfWork(textRequest2);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(6)),
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.TeamDesc}
			};

			var resultDesc = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter).ToArray();

			resultDesc.Should().Have.SameSequenceAs(new List<IPersonRequest> {textRequest2, textRequest1});
		}

		[Test]
		public void ShouldReturnRequestsWithMultisorting()
		{
			var person1 = PersonFactory.CreatePerson("A");
			var person2 = PersonFactory.CreatePerson("B");
			var person3 = PersonFactory.CreatePerson("C");

			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);

			var textRequest1 = new PersonRequest(person1, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
			var textRequest2 = new PersonRequest(person2, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
			var textRequest3 = new PersonRequest(person3, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));

			textRequest1.Subject = "Monday";
			textRequest2.Subject = "Monday";
			textRequest3.Subject = "Sunday";

			PersistAndRemoveFromUnitOfWork(textRequest1);
			PersistAndRemoveFromUnitOfWork(textRequest2);
			PersistAndRemoveFromUnitOfWork(textRequest3);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.SubjectDesc, RequestsSortingOrder.AgentNameAsc}
			};

			var resultDesc = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter).ToArray();

			resultDesc.Should().Have.SameSequenceAs(new List<IPersonRequest> {textRequest3, textRequest1, textRequest2});
		}

		[Test]
		public void ShouldReturnRequestsWithPagingRequirement()
		{
			var person1 = PersonFactory.CreatePerson("A");
			var person2 = PersonFactory.CreatePerson("B");

			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);

			for (int i = 0; i < 10; i++)
			{
				var textRequest1 = new PersonRequest(person1, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
				PersistAndRemoveFromUnitOfWork(textRequest1);
				var textRequest2 = new PersonRequest(person2, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
				PersistAndRemoveFromUnitOfWork(textRequest2);
			}

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.AgentNameDesc},
				Paging = new Paging {Skip = 10, Take = 5}
			};

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequests(filter).ToArray();

			result.Length.Should().Be.EqualTo(5);
			result.Any(request => request.Person.Name.FirstName != "A").Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldReturnCorrectRequestsTotalCountWithPaging()
		{
			var person1 = PersonFactory.CreatePerson("A");
			var person2 = PersonFactory.CreatePerson("B");

			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);

			for (int i = 0; i < 10; i++)
			{
				var textRequest1 = new PersonRequest(person1, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
				PersistAndRemoveFromUnitOfWork(textRequest1);
				var textRequest2 = new PersonRequest(person2, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
				PersistAndRemoveFromUnitOfWork(textRequest2);
			}

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.AgentNameDesc},
				Paging = new Paging {Skip = 10, Take = 5}
			};

			int count;
			var result = new PersonRequestRepository(UnitOfWork).FindAllRequests(filter, out count).ToArray();

			result.Length.Should().Be.EqualTo(5);
			count.Should().Be.EqualTo(20);
		}

		[Test]
		public void ShouldReturnBothAutoDeniedAndDeniedRequestsWhenFilteringByDenied()
		{
			var authChecker = new PersonRequestAuthorizationCheckerForTest();

			var secondAbsence = AbsenceFactory.CreateAbsence("Second Absence");
			var thirdAbsence = AbsenceFactory.CreateAbsence("Third Absence");

			PersistAndRemoveFromUnitOfWork(secondAbsence);
			PersistAndRemoveFromUnitOfWork(thirdAbsence);

			var request1 = createAbsenceRequestAndBusinessUnit(_absence);
			var request2 = createAbsenceRequestAndBusinessUnit(secondAbsence);
			var request3 = createAbsenceRequestAndBusinessUnit(thirdAbsence);

			request1.Deny(null, "Work harder", authChecker);
			request2.Approve(new ApprovalServiceForTest(), authChecker);

			request3.SetNew();

			request3.Deny(null, "Work harder", authChecker, PersonRequestDenyOption.AutoDeny);

			PersistAndRemoveFromUnitOfWork(request1);
			PersistAndRemoveFromUnitOfWork(request2);
			PersistAndRemoveFromUnitOfWork(request3);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(2008, 07, 09, 2008, 07, 20),
				RequestFilters = new Dictionary<RequestFilterField, string>
				{
					{RequestFilterField.Status, "1"} // Denied
				}
			};
			int count;
			var foundRequests = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter, out count).ToList();

			Assert.AreEqual(2, foundRequests.Count);
			Assert.IsTrue(!foundRequests.Contains(request2));
		}

		[Test]
		public void ShouldReturnRequestsOverlapOnEndDate()
		{
			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow),
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.PeriodStartDesc}
			};

			var resultDesc = simpleRequestFilter(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)), filter);

			resultDesc.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldReturnRequestsOverlapOnStartDate()
		{
			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)),
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.PeriodStartDesc}
			};

			var resultDesc = simpleRequestFilter(new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow), filter);

			resultDesc.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotReturnRequestsOverlapOnEndDate()
		{
			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow),
				ExcludeRequestsOnFilterPeriodEdge = true,
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.PeriodStartDesc}
			};

			var resultDesc = simpleRequestFilter(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)), filter);

			resultDesc.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldNotReturnRequestsOverlapOnStartDate()
		{
			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)),
				ExcludeRequestsOnFilterPeriodEdge = true,
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.PeriodStartDesc}
			};

			var resultDesc = simpleRequestFilter(new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow), filter);

			resultDesc.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldOnlyReturnRequestsStartWithinPeriod()
		{
			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(7)),
				OnlyIncludeRequestsStartingWithinPeriod = true,
				SortingOrders = new List<RequestsSortingOrder> {RequestsSortingOrder.PeriodStartDesc}
			};

			var resultDesc = simpleRequestFilter(new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(7)),
				filter);

			resultDesc.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldExcludeDeletedShiftExchangeOfferForBulletin()
		{
			var startDate = DateTime.UtcNow;

			var shiftExchangeOfferReq = createShiftExchangeOffer(startDate, startDate, startDate.AddDays(1));
			var shiftExchangeOfferReq2 = createShiftExchangeOffer(startDate, startDate, startDate.AddDays(1));
			((IDeleteTag) shiftExchangeOfferReq2).SetDeleted();

			PersistAndRemoveFromUnitOfWork(shiftExchangeOfferReq);
			PersistAndRemoveFromUnitOfWork(shiftExchangeOfferReq2);

			var foundShiftExchangeRequests =
				new PersonRequestRepository(UnitOfWork).FindShiftExchangeOffersForBulletin(new[] {_person},
					new DateOnly(startDate)).ToList();

			Assert.AreEqual(1, foundShiftExchangeRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundShiftExchangeRequests[0]));
			Assert.IsTrue(foundShiftExchangeRequests.Contains(shiftExchangeOfferReq.Request));
		}

		

		#region Request filtering test cases

		[Test]
		public void ShouldFilterRequestOnSubject()
		{
			var request1 = CreateAggregateWithCorrectBusinessUnit();
			request1.Subject = "Abc 123";
			var request2 = CreateAggregateWithCorrectBusinessUnit();
			request2.Subject = "Bcd 234";
			var request3 = CreateShiftTradeRequest("Trade With Me");
			request3.Subject = "Cde 345";

			PersistAndRemoveFromUnitOfWork(request1);
			PersistAndRemoveFromUnitOfWork(request2);
			PersistAndRemoveFromUnitOfWork(request3);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(2008, 07, 09, 2008, 07, 20),
				RequestFilters = new Dictionary<RequestFilterField, string>
				{
					{RequestFilterField.Subject, "a 2"}
				}
			};
			int count;
			var foundRequests = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter, out count).ToList();

			Assert.AreEqual(1, foundRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundRequests[0].Request));
			Assert.IsTrue(foundRequests.Contains(request1));
		}

		[Test]
		public void ShouldFilterRequestOnMessage()
		{
			var request1 = CreateAggregateWithCorrectBusinessUnit();
			request1.TrySetMessage("Abc 123");
			var request2 = CreateAggregateWithCorrectBusinessUnit();
			request2.TrySetMessage("Bcd 234");
			var request3 = CreateShiftTradeRequest("Trade With Me");
			request3.TrySetMessage("Cde 345");

			PersistAndRemoveFromUnitOfWork(request1);
			PersistAndRemoveFromUnitOfWork(request2);
			PersistAndRemoveFromUnitOfWork(request3);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(2008, 07, 09, 2008, 07, 20),
				RequestFilters = new Dictionary<RequestFilterField, string>
				{
					{RequestFilterField.Message, "A 2"}
				}
			};
			int count;
			var foundRequests = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter, out count).ToList();

			Assert.AreEqual(1, foundRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundRequests[0].Request));
			Assert.IsTrue(foundRequests.Contains(request1));
		}

		[Test]
		public void ShouldNotFilterDeletedPeople()
		{
			var personTo = PersonFactory.CreatePerson("person to");
			personTo.TerminatePerson(new DateOnly(1900, 1, 1), MockRepository.GenerateMock<IPersonAccountUpdater>());
			var personFrom = PersonFactory.CreatePerson("person from");
			((Person) personFrom).SetDeleted();
			PersistAndRemoveFromUnitOfWork(personTo);
			PersistAndRemoveFromUnitOfWork(personFrom);

			var shiftTradeRequestOfDeletedPerson = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16))
				});

			var shiftTradeRequestOfUndeletedPerson = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(personTo, personFrom, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16))
				});

			var shiftTradePersonRequestOfDeletedPerson = new PersonRequest(personFrom)
			{
				Request = shiftTradeRequestOfDeletedPerson
			};
			var shiftTradePersonRequestOfUndeletedPerson = new PersonRequest(personTo)
			{
				Request = shiftTradeRequestOfUndeletedPerson
			};
			shiftTradePersonRequestOfDeletedPerson.Pending();
			shiftTradePersonRequestOfUndeletedPerson.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequestOfDeletedPerson);
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequestOfUndeletedPerson);

			int count;
			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(2008, 07, 09, 2008, 07, 20),
			};
			var foundRequests = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter, out count).ToList();

			Assert.AreEqual(1, foundRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundRequests[0].Request));
			Assert.IsTrue(foundRequests.Contains(shiftTradePersonRequestOfUndeletedPerson));
		}

		[Test]
		public void ShouldGetBrokenBusinessRules()
		{
			var shiftTradeRequest1 = CreateShiftTradeRequest("Trade With Me");
			shiftTradeRequest1.TrySetBrokenBusinessRule(BusinessRuleFlags.DataPartOfAgentDay);
			PersistAndRemoveFromUnitOfWork(shiftTradeRequest1);
			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(2008, 07, 09, 2008, 07, 20),
			};
			int count;
			var foundRequests = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter, out count).ToList();
			Assert.AreEqual(1, foundRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundRequests[0].Request));
			Assert.IsTrue(foundRequests.Contains(shiftTradeRequest1));
			Assert.AreEqual(foundRequests[0].BrokenBusinessRules, BusinessRuleFlags.DataPartOfAgentDay);
		}

		[Test]
		public void ShouldFilterRequestOnStatus()
		{
			var request1 = CreateShiftTradeRequest("Trade With Me 1");
			var request2 = CreateShiftTradeRequest("Trade With Me 2");
			var request3 = CreateAggregateWithCorrectBusinessUnit();

			PersistAndRemoveFromUnitOfWork(request1);
			PersistAndRemoveFromUnitOfWork(request2);
			PersistAndRemoveFromUnitOfWork(request3);

			var approvalSvc = new ApprovalServiceForTest();
			var authChecker = new PersonRequestAuthorizationCheckerForTest();
			request1.Approve(approvalSvc, authChecker);
			PersistAndRemoveFromUnitOfWork(request1);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(2008, 07, 09, 2008, 07, 20),
				RequestFilters = new Dictionary<RequestFilterField, string>
				{
					{RequestFilterField.Status, "2"} // 2: Approved
				}
			};
			int count;
			var foundRequests = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter, out count).ToList();

			Assert.AreEqual(1, foundRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundRequests[0].Request));
			Assert.IsTrue(foundRequests.Contains(request1));
		}

		[Test]
		public void ShouldFilterRequestOnAbsenceType()
		{
			var secondAbsence = AbsenceFactory.CreateAbsence("Second Absence");
			var thirdAbsence = AbsenceFactory.CreateAbsence("Third Absence");

			PersistAndRemoveFromUnitOfWork(secondAbsence);
			PersistAndRemoveFromUnitOfWork(thirdAbsence);

			var request1 = createAbsenceRequestAndBusinessUnit(_absence);
			var request2 = createAbsenceRequestAndBusinessUnit(secondAbsence);
			var request3 = createAbsenceRequestAndBusinessUnit(thirdAbsence);

			PersistAndRemoveFromUnitOfWork(request1);
			PersistAndRemoveFromUnitOfWork(request2);
			PersistAndRemoveFromUnitOfWork(request3);

			var approvalSvc = new ApprovalServiceForTest();
			var authChecker = new PersonRequestAuthorizationCheckerForTest();
			request1.Approve(approvalSvc, authChecker);
			PersistAndRemoveFromUnitOfWork(request1);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(2008, 07, 09, 2008, 07, 20),
				RequestFilters = new Dictionary<RequestFilterField, string>
				{
					{RequestFilterField.Absence, $"{_absence.Id} {secondAbsence.Id}"}
				}
			};
			int count;
			var foundRequests = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter, out count).ToList();

			Assert.AreEqual(2, foundRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundRequests[0].Request));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundRequests[1].Request));
			Assert.IsTrue(foundRequests.Contains(request1));
			Assert.IsTrue(foundRequests.Contains(request2));
		}

		[Test]
		public void ShouldFilterRequestOnMultipleCriteria()
		{
			var request1 = CreateAggregateWithCorrectBusinessUnit();
			request1.Subject = "Abc 123";
			request1.TrySetMessage("Abc 123");
			var request2 = CreateAggregateWithCorrectBusinessUnit();
			request2.Subject = "Bcd 234";
			request2.TrySetMessage("Bcd 234");
			var request3 = CreateShiftTradeRequest("Trade With Me");
			request3.Subject = "Cde 234";
			request3.TrySetMessage("Cde 345");
			var request4 = new PersonRequest(_person,
				new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)))
			{
				Subject = "Def 456"
			};
			request4.TrySetMessage("Def 456");

			PersistAndRemoveFromUnitOfWork(request1);
			PersistAndRemoveFromUnitOfWork(request2);
			PersistAndRemoveFromUnitOfWork(request3);
			PersistAndRemoveFromUnitOfWork(request4);

			var approvalSvc = new ApprovalServiceForTest();
			var authChecker = new PersonRequestAuthorizationCheckerForTest();
			request2.Approve(approvalSvc, authChecker);
			PersistAndRemoveFromUnitOfWork(request2);

			var filter = new RequestFilter
			{
				Period = new DateTimePeriod(2008, 07, 09, 2008, 07, 20),
				RequestFilters = new Dictionary<RequestFilterField, string>
				{
					{RequestFilterField.Subject, "b 2"},
					{RequestFilterField.Message, "c 3"},
					{RequestFilterField.Absence, _absence.Id.ToString()},
					{RequestFilterField.Status, "2"} // 2: Approved
				}
			};
			int count;
			var foundRequests = new PersonRequestRepository(UnitOfWork)
				.FindAllRequests(filter, out count).ToList();

			Assert.AreEqual(1, foundRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundRequests[0].Request));
			Assert.IsTrue(foundRequests.Contains(request2));
		}

		#endregion Request filtering test cases

		private IPersonRequest createAndPersistTextRequest(IPerson person, DateTimePeriod period)
		{
			var textRequest = new PersonRequest(person, new TextRequest(period));
			PersistAndRemoveFromUnitOfWork(textRequest);
			return textRequest;
		}

		private IPerson createAndPersistPerson(string name, DateOnly personPeriodStartDate)
		{
			var person = PersonFactory.CreatePerson(name);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(personPeriodStartDate,
				PersonContractFactory.CreatePersonContract(_contract, _partTimePercentage, _contractSchedule), _team));
			PersistAndRemoveFromUnitOfWork(person);

			return person;
		}

		private IEnumerable<IPersonRequest> simpleRequestFilter(DateTimePeriod existingRequestPeriod, RequestFilter filter)
		{
			var person1 = PersonFactory.CreatePerson("A");

			PersistAndRemoveFromUnitOfWork(person1);

			var textRequest1 = new PersonRequest(person1, new TextRequest(existingRequestPeriod));

			PersistAndRemoveFromUnitOfWork(textRequest1);

			return new PersonRequestRepository(UnitOfWork).FindAllRequests(filter).ToArray();
		}


		[Test]
		public void ShouldLoadWaitlistedAbsenceRequestsOnGivenPeriod()
		{
			var absence = AbsenceFactory.CreateAbsence("Football");
			PersistAndRemoveFromUnitOfWork(absence);

			var wcs = new WorkflowControlSet()
			{
				Name = "dd",
				AbsenceRequestWaitlistEnabled = true,
				AbsenceRequestWaitlistProcessOrder = WaitlistProcessOrder.BySeniority

			};
			wcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 2, 1, 2099, 2, 28),
				Period = new DateOnlyPeriod(2016, 2, 1, 2099, 2, 28),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator() ,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			
			});
			var wcs2 = new WorkflowControlSet()
			{
				Name = "No Waitlist",
				AbsenceRequestWaitlistEnabled = false
			};
			wcs2.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 2, 1, 2099, 2, 28),
				Period = new DateOnlyPeriod(2016, 2, 1, 2099, 2, 28),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			PersistAndRemoveFromUnitOfWork(new[] { wcs, wcs2 });
			
			var person = PersonFactory.CreatePerson("Asad");
			person.WorkflowControlSet = wcs;
			var person2 = PersonFactory.CreatePerson("Ali");
			person2.WorkflowControlSet = wcs2;

			PersistAndRemoveFromUnitOfWork(new [] {person, person2});

			
			IAbsenceRequest absenceRequest1 = new AbsenceRequest(absence, new DateTimePeriod(new DateTime(2016,03,02,10,0,0,DateTimeKind.Utc), new DateTime(2016, 03, 05, 12, 0, 0, DateTimeKind.Utc)));
			IAbsenceRequest absenceRequest2 = new AbsenceRequest(absence, new DateTimePeriod(new DateTime(2016,03,02,10,0,0,DateTimeKind.Utc), new DateTime(2016, 03, 03, 12, 0, 0, DateTimeKind.Utc)));
			IAbsenceRequest absenceRequest3 = new AbsenceRequest(absence, new DateTimePeriod(new DateTime(2016,02,28,10,0,0,DateTimeKind.Utc), new DateTime(2016, 03, 03, 12, 0, 0, DateTimeKind.Utc)));
			IAbsenceRequest absenceRequest4 = new AbsenceRequest(absence, new DateTimePeriod(new DateTime(2016,03,2,11,0,0,DateTimeKind.Utc), new DateTime(2016, 03, 2, 18, 0, 0, DateTimeKind.Utc)));
			IAbsenceRequest absenceRequest5 = new AbsenceRequest(absence, new DateTimePeriod(new DateTime(2016,03,5,11,0,0,DateTimeKind.Utc), new DateTime(2016, 03, 6, 18, 0, 0, DateTimeKind.Utc)));
			IAbsenceRequest absenceRequest6 = new AbsenceRequest(absence, new DateTimePeriod(new DateTime(2016,02,28,11,0,0,DateTimeKind.Utc), new DateTime(2016, 03, 8, 18, 0, 0, DateTimeKind.Utc)));

			IPersonRequest request1 = new PersonRequest(person, absenceRequest1);
			request1.Pending();
			request1.Deny(request1.Person, "waitlisted", new PersonRequestCheckAuthorization(),true);
			PersistAndRemoveFromUnitOfWork(request1);


			IPersonRequest request2 = new PersonRequest(person, absenceRequest2);
			request2.Pending();
			request2.Deny(request1.Person, "waitlisted",  new PersonRequestCheckAuthorization(), true);
			PersistAndRemoveFromUnitOfWork(request2);

			IPersonRequest request3 = new PersonRequest(person, absenceRequest3);
			request3.Pending();
			request3.Deny(request1.Person, "waitlisted", new PersonRequestCheckAuthorization(), true);
			PersistAndRemoveFromUnitOfWork(request3);

			IPersonRequest request5 = new PersonRequest(person, absenceRequest5);
			request5.Pending();
			request5.Deny(request1.Person, "waitlisted", new PersonRequestCheckAuthorization(), true);
			PersistAndRemoveFromUnitOfWork(request5);

			IPersonRequest request6 = new PersonRequest(person, absenceRequest6);
			request6.Pending();
			request6.Deny(request1.Person, "waitlisted", new PersonRequestCheckAuthorization(), true);
			PersistAndRemoveFromUnitOfWork(request6);

			IPersonRequest request4 = new PersonRequest(person2, absenceRequest4);
			request4.Pending();
			request4.Deny(request1.Person, "Not waitlisted", new PersonRequestCheckAuthorization(), true);
			PersistAndRemoveFromUnitOfWork(request4);


			var waitlistRequestsIds = new PersonRequestRepository(UnitOfWork).GetWaitlistRequests(new DateTimePeriod(new DateTime(2016,03,01,0,0,0,DateTimeKind.Utc), new DateTime(2016, 03, 04, 0, 0, 0, DateTimeKind.Utc))).ToArray();
			waitlistRequestsIds.Count().Should().Be.EqualTo(4);
			CollectionAssert.AreEquivalent(waitlistRequestsIds,
				new List<Guid>() {request1.Id.GetValueOrDefault(), request2.Id.GetValueOrDefault(), request3.Id.GetValueOrDefault(), request6.Id.GetValueOrDefault()});
		}
	}
}