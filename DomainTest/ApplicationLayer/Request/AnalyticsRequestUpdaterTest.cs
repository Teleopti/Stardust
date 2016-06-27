using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Request;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Request
{
	[TestFixture]
	public class AnalyticsRequestUpdaterTest
	{
		private AnalyticsRequestUpdater _target;
		private IPersonRequestRepository _personRequestRepository;
		private FakeAnalyticsRequestRepository _analyticsRequestRepository;
		private IAnalyticsDateRepository _analyticsDateRepository;
		private IAnalyticsPersonPeriodRepository _personPeriodRepository;
		private IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private IAnalyticsAbsenceRepository _analyticsAbsenceRepository;

		[SetUp]
		public void Setup()
		{
			_personRequestRepository = new FakePersonRequestRepository();
			_analyticsRequestRepository = new FakeAnalyticsRequestRepository();
			_analyticsDateRepository = new FakeAnalyticsDateRepository(new DateTime(2000, 1, 1),DateTime.Today+TimeSpan.FromDays(365));
			_personPeriodRepository = new FakeAnalyticsPersonPeriodRepository();
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_analyticsAbsenceRepository = new FakeAnalyticsAbsenceRepository();

			_target = new AnalyticsRequestUpdater(_personRequestRepository, _analyticsRequestRepository, _analyticsDateRepository, _analyticsBusinessUnitRepository, _personPeriodRepository, _analyticsAbsenceRepository);
		}

		[Test]
		public void MissingRequestInAppDatabaseDoesNothing()
		{
			_target.Handle(new PersonRequestChangedEvent { PersonRequestId = Guid.NewGuid(), LogOnBusinessUnitId = Guid.NewGuid()});

			_analyticsRequestRepository.AnalyticsRequestedDays.Should().Be.Empty();
			_analyticsRequestRepository.AnalyticsRequests.Should().Be.Empty();
		}

		[Test, ExpectedException(typeof(PersonPeriodMissingInAnalyticsException))]
		public void MissingPersonPeriodInAnalyticsDatabaseThrows()
		{
			var personRequest = new PersonRequestFactory().CreatePersonRequest();
			personRequest.SetId(Guid.NewGuid());
			personRequest.Request.SetId(Guid.NewGuid());
			_personRequestRepository.Add(personRequest);
			_target.Handle(new PersonRequestChangedEvent { PersonRequestId = personRequest.Id.GetValueOrDefault(), LogOnBusinessUnitId = Guid.NewGuid() });
		}

		[Test]
		public void ShouldAddRequestAndRequestedDay()
		{
			var person = PersonFactory.CreatePerson("Test");
			person.SetId(Guid.NewGuid());
			var personRequestFactory = new PersonRequestFactory
			{
				Request = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1)))
			};
			var personRequest = personRequestFactory.CreatePersonRequest(person);
			personRequest.SetId(Guid.NewGuid());
			personRequest.Request.SetId(Guid.NewGuid());
			_personRequestRepository.Add(personRequest);

			_personPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod
			{
				PersonCode = person.Id.GetValueOrDefault(),
				PersonId = 1,
				ValidFromDate = DateTime.UtcNow - TimeSpan.FromDays(10),
				ValidToDate = DateTime.UtcNow + TimeSpan.FromDays(10)
			});

			_target.Handle(new PersonRequestDeletedEvent { PersonRequestId = personRequest.Id.GetValueOrDefault(), LogOnBusinessUnitId = Guid.NewGuid() });

			_analyticsRequestRepository.AnalyticsRequestedDays.Should().Not.Be.Empty();
			_analyticsRequestRepository.AnalyticsRequests.Should().Not.Be.Empty();
		}


		[Test]
		public void ShouldDeleteRequestAndRequestedDay()
		{
			var person = PersonFactory.CreatePerson("Test");
			person.SetId(Guid.NewGuid());
			var personRequestFactory = new PersonRequestFactory
			{
				Request = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1)))
			};
			var personRequest = (PersonRequest)personRequestFactory.CreatePersonRequest(person);
			personRequest.SetDeleted();
			personRequest.SetId(Guid.NewGuid());
			personRequest.Request.SetId(Guid.NewGuid());
			_personRequestRepository.Add(personRequest);
			_analyticsRequestRepository.AnalyticsRequestedDays.Add(new AnalyticsRequestedDay {RequestCode = personRequest.Id.GetValueOrDefault() });
			_analyticsRequestRepository.AnalyticsRequests.Add(new AnalyticsRequest {RequestCode = personRequest.Id.GetValueOrDefault() });

			_personPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod
			{
				PersonCode = person.Id.GetValueOrDefault(),
				PersonId = 1,
				ValidFromDate = DateTime.UtcNow - TimeSpan.FromDays(10),
				ValidToDate = DateTime.UtcNow + TimeSpan.FromDays(10)
			});

			_target.Handle(new PersonRequestChangedEvent { PersonRequestId = personRequest.Id.GetValueOrDefault(), LogOnBusinessUnitId = Guid.NewGuid() });

			_analyticsRequestRepository.AnalyticsRequestedDays.Should().Be.Empty();
			_analyticsRequestRepository.AnalyticsRequests.Should().Be.Empty();
		}
	}
}