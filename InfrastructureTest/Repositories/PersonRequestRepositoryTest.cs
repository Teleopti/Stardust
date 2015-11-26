using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
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

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _defaultScenario = ScenarioFactory.CreateScenarioAggregate("Default", true);
            _person = PersonFactory.CreatePerson("sdfoj");
            _absence = AbsenceFactory.CreateAbsence("Sick leave");

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

	    private IPersonRequest createAbsenceRequestAndBusinessUnit()
	    {
		    var period = new DateTimePeriod(
			    new DateTime(2008, 7, 10, 0, 0, 0, DateTimeKind.Utc),
			    new DateTime(2008, 7, 11, 0, 0, 0, DateTimeKind.Utc));
		    IPersonRequest request = new PersonRequest(_person);
		    IAbsenceRequest absenceRequest = new AbsenceRequest(_absence, period);

		    request.Request = absenceRequest;
		    request.Pending();

		    return request;
	    }
		
		
		private IPersonRequest createShiftExchangeOffer(DateTime startDate)
	    {
		    IPersonRequest request = new PersonRequest(_person);
			var currentShift = ScheduleDayFactory.Create(new DateOnly(2008, 5, 1), _person);

			var dayFilterCriteria = new ScheduleDayFilterCriteria(ShiftExchangeLookingForDay.WorkingShift,
				new DateTimePeriod(startDate, startDate.AddDays(1)));
			var offer = new ShiftExchangeOffer(currentShift,
				new ShiftExchangeCriteria(new DateOnly(2008, 7, 9), dayFilterCriteria), ShiftExchangeOfferStatus.Pending);

			request.Request = offer;
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
            Assert.AreEqual(((IAbsenceRequest)org.Request).Absence,
                            ((IAbsenceRequest)loadedAggregateFromDatabase.Request).Absence);
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
			   new ShiftTradeSwapDetail(personFrom, _person, requestDate,requestDate)
		    });
			var shiftTradePersonRequest = new PersonRequest(personFrom) { Request = shiftTradeRequest };
			var textRequest = new PersonRequest(_person, new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
		    var absenceRequest = new PersonRequest(_person, new AbsenceRequest(absence, new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow)));
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
			PersistAndRemoveFromUnitOfWork (offerRequest);

			DateTimePeriod period = new DateTimePeriod(2008, 04, 1, 2008, 07, 20);
			IList<IPersonRequest> foundRequests = new PersonRequestRepository(UnitOfWork).Find(_person, period);
			Assert.AreEqual(1, foundRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundRequests[0].Request));
			Assert.IsTrue(foundRequests.Contains(offerRequest));
	    }

	    [Test]
	    public void FindPersonRequestByRequestType()
	    {
			var startDate = new DateTime(2008, 4, 1, 0, 0, 0, DateTimeKind.Utc);

			var shiftExchangeOfferPersonRequest = createShiftExchangeOffer(startDate);
		    var shiftTradePersonRequest = CreateShiftTradeRequest ("Test");

			PersistAndRemoveFromUnitOfWork(shiftExchangeOfferPersonRequest);
			PersistAndRemoveFromUnitOfWork (shiftTradePersonRequest);

			var period = new DateTimePeriod(2000, 04, 1, 2014, 08, 20);
			var foundShiftExchangeRequests = new PersonRequestRepository(UnitOfWork).Find<ShiftExchangeOffer>(_person, period);
			var foundShiftTradeRequests = new PersonRequestRepository(UnitOfWork).Find<ShiftTradeRequest>(_person, period);
			
			Assert.AreEqual(1, foundShiftExchangeRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundShiftExchangeRequests[0].Request));
			Assert.IsTrue(foundShiftExchangeRequests.Contains(shiftExchangeOfferPersonRequest));
			
			Assert.AreEqual(1, foundShiftTradeRequests.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(foundShiftTradeRequests[0].Request));
			Assert.IsTrue(foundShiftTradeRequests.Contains(shiftTradePersonRequest));
			
	    }	    	
		
		[Test]
		public void FindPendingPersonRequestByRequestTypeAndStartDate()
		{
			var startDate = new DateTime (2008, 4, 1, 0, 0, 0, DateTimeKind.Utc);
			var startDate2 = startDate.AddDays (1);

			var shiftExchangeOfferPersonRequest = createShiftExchangeOffer(startDate);
			var shiftExchangeOfferPersonRequest2 = createShiftExchangeOffer(startDate2);

			shiftExchangeOfferPersonRequest2.Deny (null, "bla", new PersonRequestCheckAuthorization());

			PersistAndRemoveFromUnitOfWork(shiftExchangeOfferPersonRequest);
			PersistAndRemoveFromUnitOfWork(shiftExchangeOfferPersonRequest2);
			
			var foundShiftExchangeRequests = new PersonRequestRepository(UnitOfWork).FindByStatus<ShiftExchangeOffer>(_person, startDate, 0);

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
            IList<IPersonRequest> foundRequests = new PersonRequestRepository(UnitOfWork).Find(_person,period);

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
			foundRequest.Count().Should().Be.EqualTo(1);
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

			var foundRequests = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgent(_person, new Paging { Take = 10 });

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
			IPersonRequest personRequestWithAbsenceRequest1 = CreateAggregateWithCorrectBusinessUnit();
			IPersonRequest personRequestWithAbsenceRequest2 = CreateAggregateWithCorrectBusinessUnit();
			IPersonRequest personRequestWithAbsenceRequest3 = CreateAggregateWithCorrectBusinessUnit();

			// ouch! better way to modify updated on?
			PersistAndRemoveFromUnitOfWork(personRequestWithAbsenceRequest1);
			SetUpdatedOnForRequest(personRequestWithAbsenceRequest1,-2);
			PersistAndRemoveFromUnitOfWork(personRequestWithAbsenceRequest2);
			SetUpdatedOnForRequest(personRequestWithAbsenceRequest2, -1);
			PersistAndRemoveFromUnitOfWork(personRequestWithAbsenceRequest3);

			var results = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgent(_person, new Paging { Take = 1, Skip = 1 });

			results.Should().Have.Count.EqualTo(1);
			results.Single().Should().Be.EqualTo(personRequestWithAbsenceRequest2);
		}

    	private void SetUpdatedOnForRequest(IPersonRequest personRequest,int minutes)
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
				                  new AbsenceRequest(_absence, new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(3)))
					);
			var personRequestNotInperiod =
				new PersonRequest(_person,
				                  new AbsenceRequest(_absence, new DateTimePeriod(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2)))
					);

			PersistAndRemoveFromUnitOfWork(personRequestInPeriod);
			PersistAndRemoveFromUnitOfWork(personRequestNotInperiod);

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgent(_person, new DateTimePeriod(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1)));

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
            DateTimePeriod periodToLookFor = new DateTimePeriod(updatedOn.Subtract(TimeSpan.FromDays(1)),updatedOn.AddDays(1));
            DateTimePeriod periodOutside = new DateTimePeriod(updatedOn.AddDays(2),updatedOn.AddDays(4));
           
            IList<IPersonRequest> foundRequests = new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(_person,periodToLookFor);
            IList<IPersonRequest> requestWithinOutsidePeriod = new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(_person, periodOutside);

            int actualValue = foundRequests.Count;
            Assert.AreEqual(4, actualValue);
            Assert.AreEqual(2,requestWithinOutsidePeriod.Count(r=>r.IsPending));
            Assert.AreEqual(2,requestWithinOutsidePeriod.Count);

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

			IList<IPersonRequest> foundRequests = new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(_person, insidePeriod);

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
          
            IEnumerable<IPersonRequest> requestWithinOutsidePeriod = new PersonRequestRepository(UnitOfWork).FindPersonRequestWithinPeriod(personRequest.Request.Period);
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
                        new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16),new DateOnly(2008, 7, 16)),
                        new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 18),new DateOnly(2008, 7, 18))
                    });

            IPersonRequest shiftTradePersonRequest = new PersonRequest(personTo);
            shiftTradePersonRequest.Request = shiftTradeRequest;
            shiftTradePersonRequest.Pending();
            PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);


            IShiftTradeRequest shiftTradeRequest2 = new ShiftTradeRequest(
               new List<IShiftTradeSwapDetail>
                    {
                        new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 20),new DateOnly(2008, 7, 20)),
                        new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 21),new DateOnly(2008, 7, 21))
                    });

            IPersonRequest shiftTradePersonRequest2 = new PersonRequest(personTo);
            shiftTradePersonRequest2.Request = shiftTradeRequest2;
            shiftTradePersonRequest2.Pending();
            PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest2);

            IEnumerable<IPersonRequest> requestWithinOutsidePeriod = new PersonRequestRepository(UnitOfWork).FindPersonRequestWithinPeriod(shiftTradeRequest2.Period);
            Assert.AreEqual(1, requestWithinOutsidePeriod.Count());
        }


        [Test]
        public void VerifyRequestStatusIsNotNew()
        {
            IPersonRequest request = new PersonRequest(_person);
            IAbsenceRequest absenceRequest = new AbsenceRequest(_absence,
                                                               new DateTimePeriod(
                                                                   new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc),
                                                                   new DateTime(2008, 7, 19, 0, 0, 0, DateTimeKind.Utc)));

            request.Request = absenceRequest;
            
            PersistAndRemoveFromUnitOfWork(request);
            var requestWithStatusNew = new PersonRequestRepository(UnitOfWork).FindPersonRequestWithinPeriod(request.Request.Period);
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
                        new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16),new DateOnly(2008, 7, 16)),
                        new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 18),new DateOnly(2008, 7, 18))
                    });


            IPersonRequest shiftTradePersonRequest = new PersonRequest(personTo);
            shiftTradePersonRequest.Request = shiftTradeRequest;
            shiftTradePersonRequest.Pending();
            PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

            IEnumerable<IPersonRequest> requestWithinOutsidePeriod = new PersonRequestRepository(UnitOfWork).FindPersonRequestWithinPeriod(shiftTradePersonRequest.Request.Period);
            var firstResult = requestWithinOutsidePeriod.Single();
            LazyLoadingManager.IsInitialized(firstResult.Request).Should().Be.True();
            LazyLoadingManager.IsInitialized(((IShiftTradeRequest)firstResult.Request).ShiftTradeSwapDetails).Should().Be.True();
            LazyLoadingManager.IsInitialized(((IShiftTradeRequest)firstResult.Request).ShiftTradeSwapDetails[0]).Should().Be.True();
        }


        [Test]
        public void VerifyFindAllRequestsForAgentsForAPeriod()
        {
            IPersonRequest personRequest = CreateAggregateWithCorrectBusinessUnit();
            IPersonRequest pendingPersonRequest = CreateAggregateWithCorrectBusinessUnit();
            IPersonRequest shiftTradePersonRequest;
            IPersonRequest pendingShiftTradePersonRequest;

            IPerson personTo = PersonFactory.CreatePerson("vjiosd");
            personTo.Name = new Name("mala", "mala");
            PersistAndRemoveFromUnitOfWork(personTo);

            IPerson anotherPerson = PersonFactory.CreatePerson("Smet");
            anotherPerson.Name = new Name("Tom", "Jones");
            PersistAndRemoveFromUnitOfWork(anotherPerson);

            IPerson anotherPerson2 = PersonFactory.CreatePerson("Smet...");
            anotherPerson2.Name = new Name("Kirk", "Douglas");
            PersistAndRemoveFromUnitOfWork(anotherPerson2);

            IList<IPerson> persons = new List<IPerson>{_person, anotherPerson, anotherPerson2};
            
            IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
                new List<IShiftTradeSwapDetail>
                    {
                        new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16),new DateOnly(2008, 7, 16))
                    });

            IShiftTradeRequest shiftTradeRequest2 = new ShiftTradeRequest(
               new List<IShiftTradeSwapDetail>
                    {
                        new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16))
                    });

            IPersonRequest textRequest = new PersonRequest(anotherPerson);
            textRequest.Request = new TextRequest(new DateTimePeriod(new DateTime(2008, 7, 17,0,0,0,DateTimeKind.Utc), new DateTime(2008, 7, 18,0,0,0,DateTimeKind.Utc)));
            textRequest.Pending();

            IPersonRequest textRequestDenied = new PersonRequest(anotherPerson2);
            textRequestDenied.Request = new TextRequest(new DateTimePeriod(new DateTime(2008, 7, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 7, 18, 0, 0, 0, DateTimeKind.Utc)));
            textRequestDenied.Deny(null, null, new PersonRequestAuthorizationCheckerForTest());

            pendingShiftTradePersonRequest = new PersonRequest(personTo);
            
            shiftTradePersonRequest = new PersonRequest(personTo);
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

            DateTime updatedOn = personRequest.UpdatedOn.GetValueOrDefault();
            DateTimePeriod periodToLookFor = new DateTimePeriod(updatedOn.Subtract(TimeSpan.FromDays(1)), updatedOn.AddDays(1));
            DateTimePeriod periodOutside = new DateTimePeriod(updatedOn.AddDays(2), updatedOn.AddDays(4));

            IList<IPersonRequest> foundRequests = new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons, periodToLookFor);
            IList<IPersonRequest> requestWithinOutsidePeriod = new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons, periodOutside);

            int actualValue = foundRequests.Count;
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
            IList<IPersonRequest> personRequestList = new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(personList, new DateTimePeriod(2010, 1, 1, 2010, 1, 1));
        
            Assert.IsNotNull(personRequestList);
        }

        [Test]
        public void VerifyLazyLoadingOfShiftTradeDetails()
        {
            IPerson personTo = PersonFactory.CreatePerson("vjiosd");
            personTo.Name = new Name("mala", "mala");
            PersistAndRemoveFromUnitOfWork(personTo);

            IList<IPerson> persons = new List<IPerson> { _person, personTo };

            IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
                new List<IShiftTradeSwapDetail>
                    {
                        new ShiftTradeSwapDetail(personTo, _person, new DateOnly(2008, 7, 16),new DateOnly(2008, 7, 16))
                    });
            IPersonRequest shiftTradePersonRequest = new PersonRequest(personTo);
            shiftTradePersonRequest.Request = shiftTradeRequest;
            PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);
            DateTime updatedOn = shiftTradePersonRequest.UpdatedOn.GetValueOrDefault();
            DateTimePeriod periodToLookFor = new DateTimePeriod(updatedOn.Subtract(TimeSpan.FromDays(1)), updatedOn.AddDays(1));

            IList<IPersonRequest> foundRequests = new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons, periodToLookFor);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(((IShiftTradeRequest)foundRequests[0].Request).ShiftTradeSwapDetails));
        }

        [Test]
        public void VerifyCanModifyCollectionOfRequestsForAgent()
        {
            IPersonRequest request1 = CreateAggregateWithCorrectBusinessUnit();
            
            PersistAndRemoveFromUnitOfWork(request1);

            IAbsenceRequest newAbsenceRequest = new AbsenceRequest(_absence,new DateTimePeriod(
                                                                   new DateTime(2008, 8, 16, 0, 0, 0, DateTimeKind.Utc),
                                                                   new DateTime(2008, 8, 19, 0, 0, 0, DateTimeKind.Utc)));

            request1.Request=newAbsenceRequest;

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

            IPersonRequest loaded = rep.Get(request1.Id.GetValueOrDefault());

            loaded.Request = null;// new AbsenceRequest(_absence, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)); //nytt request
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

            IList list = Session.CreateCriteria(typeof(PushMessageDialogue)).List();
            Assert.AreEqual(1, list.Count);

            Session.Delete("from PersonRequest");
            Session.Delete("from AbsenceRequest");
            Session.Delete("from Absence");
            Session.Delete("from PushMessageDialogue");
            Session.Delete("from PushMessage");
					((IDeleteTag)_person).SetDeleted();
					Session.Update(_person);
					((IDeleteTag)_defaultScenario).SetDeleted();
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

			 Assert.Throws<DataSourceException>(() =>
															rep.Remove(request));
		 }

		[Test]
		public void ShouldNotIncludeShiftTradeOfDeletedPersonTo()
		{
			var personTo = PersonFactory.CreatePerson("person to");
			((IDeleteTag)personTo).SetDeleted();
			var personFrom = PersonFactory.CreatePerson("person from");
			PersistAndRemoveFromUnitOfWork(personTo);
			PersistAndRemoveFromUnitOfWork(personFrom);

			var persons = new List<IPerson> { personFrom, personTo };

			var shiftTradeRequest = new ShiftTradeRequest(
					new List<IShiftTradeSwapDetail>
                {
                    new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(2008, 7, 16),new DateOnly(2008, 7, 16))
                });
			var shiftTradePersonRequest = new PersonRequest(personFrom) {Request = shiftTradeRequest};
			shiftTradePersonRequest.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

			new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons, new DateTimePeriod(2000, 1, 1, 2010, 1, 1))
				    .Should().Be.Empty();
		}

		[Test]
		public void ShouldNotIncludeShiftTradeOfTerminatedPersonTo()
		{
			var personTo = PersonFactory.CreatePerson("person to");
			personTo.TerminatePerson(new DateOnly(1900,1,1), MockRepository.GenerateMock<IPersonAccountUpdater>());
			var personFrom = PersonFactory.CreatePerson("person from");
			PersistAndRemoveFromUnitOfWork(personTo);
			PersistAndRemoveFromUnitOfWork(personFrom);

			var persons = new List<IPerson> { personFrom, personTo };

			var shiftTradeRequest = new ShiftTradeRequest(
					new List<IShiftTradeSwapDetail>
                {
                    new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(2008, 7, 16),new DateOnly(2008, 7, 16))
                });
			var shiftTradePersonRequest = new PersonRequest(personFrom) { Request = shiftTradeRequest };
			shiftTradePersonRequest.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

			new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons, new DateTimePeriod(2000, 1, 1, 2010, 1, 1))
							.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotIncludeShiftTradeOfDeletedPersonFrom()
		{
			var personTo = PersonFactory.CreatePerson("person to");
			var personFrom = PersonFactory.CreatePerson("person from");
			((IDeleteTag)personFrom).SetDeleted();
			PersistAndRemoveFromUnitOfWork(personTo);
			PersistAndRemoveFromUnitOfWork(personFrom);

			var persons = new List<IPerson> { personFrom, personTo };

			var shiftTradeRequest = new ShiftTradeRequest(
					new List<IShiftTradeSwapDetail>
                {
                    new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(2008, 7, 16),new DateOnly(2008, 7, 16))
                });
			var shiftTradePersonRequest = new PersonRequest(personFrom) { Request = shiftTradeRequest };
			shiftTradePersonRequest.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

			new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons, new DateTimePeriod(2000, 1, 1, 2010, 1, 1))
							.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotIncludeShiftTradeOfTerminatedPersonFrom()
		{
			var personTo = PersonFactory.CreatePerson("person to");
			var personFrom = PersonFactory.CreatePerson("person from");
			personFrom.TerminatePerson(new DateOnly(1900,1,1), MockRepository.GenerateMock<IPersonAccountUpdater>());
			PersistAndRemoveFromUnitOfWork(personTo);
			PersistAndRemoveFromUnitOfWork(personFrom);

			var persons = new List<IPerson> { personFrom, personTo };

			var shiftTradeRequest = new ShiftTradeRequest(
					new List<IShiftTradeSwapDetail>
                {
                    new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(2008, 7, 16),new DateOnly(2008, 7, 16))
                });
			var shiftTradePersonRequest = new PersonRequest(personFrom) { Request = shiftTradeRequest };
			shiftTradePersonRequest.Pending();
			PersistAndRemoveFromUnitOfWork(shiftTradePersonRequest);

			new PersonRequestRepository(UnitOfWork).FindAllRequestModifiedWithinPeriodOrPending(persons, new DateTimePeriod(2000, 1, 1, 2010, 1, 1))
							.Should().Be.Empty();
		}

		[Test]
		public void ShouldOnlyGetShiftTradeRequests()
		{
			var paging = new Paging { Skip = 0, Take = 5 };
			setUpGetRequestsByTypeTests();

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgentByType(_person, paging, RequestType.ShiftTradeRequest);
			result.Single().Request.RequestType.Should().Be(RequestType.ShiftTradeRequest);

		}

		[Test]
		public void ShouldOnlyGetTextRequests()
		{
			var paging = new Paging { Skip = 0, Take = 5 };
			setUpGetRequestsByTypeTests();

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgentByType(_person, paging, RequestType.TextRequest);
			result.Single().Request.RequestType.Should().Be(RequestType.TextRequest);
		}

		[Test]
		public void ShouldOnlyGetAbsenceRequests()
		{
			var paging = new Paging { Skip = 0, Take = 5 };
			setUpGetRequestsByTypeTests();

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgentByType(_person, paging, RequestType.AbsenceRequest);
			result.Single().Request.RequestType.Should().Be(RequestType.AbsenceRequest);
		}

		[Test]
		public void ShouldOnlyGetExchangeOfferRequests()
		{
			var paging = new Paging { Skip = 0, Take = 5 };
			setUpGetRequestsByTypeTests();

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgentByType(_person, paging, RequestType.ShiftExchangeOffer);
			result.Single().Request.RequestType.Should().Be(RequestType.ShiftExchangeOffer);
		}

		[Test]
		public void ShouldGetShiftTradeAndShiftExchangeOfferRequests()
		{
			var paging = new Paging { Skip = 0, Take = 5 };
			setUpGetRequestsByTypeTests();

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequestsForAgentByType(_person, paging, RequestType.ShiftExchangeOffer, RequestType.ShiftTradeRequest).ToArray();
			result.Count().Should().Be(2);
			Assert.IsTrue(result.Any(request => request.Request.RequestType == RequestType.ShiftExchangeOffer));
			Assert.IsTrue(result.Any(request => request.Request.RequestType == RequestType.ShiftTradeRequest));
		}

		[Test]
		public void ShouldReturnAllRequestsInPeriod()
		{
			setUpGetRequestsByTypeTests();

			var result = new PersonRequestRepository(UnitOfWork).FindAllRequests(new DateTimePeriod(DateTime.UtcNow.AddDays(-1),DateTime.UtcNow.AddDays(1))).ToArray();
			result.Count().Should().Be(2);
		}
	}
    
}
