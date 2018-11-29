using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Request;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Request
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsRequestUpdaterTest : IExtendSystem
	{
		public AnalyticsRequestUpdater Target;
		public IPersonRequestRepository PersonRequestRepository;
		public FakeAnalyticsRequestRepository AnalyticsRequestRepository;
		public FakeAnalyticsDateRepository AnalyticsDateRepository;
		public IAnalyticsPersonPeriodRepository PersonPeriodRepository;
		public IAnalyticsBusinessUnitRepository AnalyticsBusinessUnitRepository;
		public IAnalyticsAbsenceRepository AnalyticsAbsenceRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AnalyticsRequestUpdater>();
		}

		[Test]
		public void MissingDateInAnalyticsDatabaseThrows()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.BusinessUnitUsedInTest);
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2000, 1, 1));

			Target = new AnalyticsRequestUpdater(PersonRequestRepository, AnalyticsRequestRepository, AnalyticsDateRepository, AnalyticsBusinessUnitRepository, PersonPeriodRepository, AnalyticsAbsenceRepository);

			var person = PersonFactory.CreatePerson("Test").WithId();
			var request = new TextRequest(new DateTimePeriod(new DateTime(1999, 1, 2, 0, 0, 0, DateTimeKind.Utc), new DateTime(1999, 1, 2, 0, 0, 0, DateTimeKind.Utc))).WithId();
			var personRequest = new PersonRequest(person, request).WithId();
			personRequest.SetBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			PersonRequestRepository.Add(personRequest);

			PersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonCode = person.Id.GetValueOrDefault(),
				PersonId = 1,
				ValidFromDate = new DateTime(1999, 1, 2, 0, 0, 0, DateTimeKind.Utc),
				ValidToDate = new DateTime(1999, 1, 3, 0, 0, 0, DateTimeKind.Utc)
			});

			Assert.Throws<DateMissingInAnalyticsException>(() =>
				Target.Handle(new PersonRequestChangedEvent
				{
					PersonRequestId = personRequest.Id.GetValueOrDefault(),
					LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
				}));
		}

		[Test]
		public void MissingRequestInAppDatabaseDoesNothing()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.BusinessUnitUsedInTest);
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(30), DateTime.Today + TimeSpan.FromDays(30));

			Target.Handle(new PersonRequestChangedEvent
			{
				PersonRequestId = Guid.NewGuid(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			AnalyticsRequestRepository.AnalyticsRequestedDays.Should().Be.Empty();
			AnalyticsRequestRepository.AnalyticsRequests.Should().Be.Empty();
		}

		[Test]
		public void MissingPersonPeriodInAnalyticsDatabaseDoesNothing()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.BusinessUnitUsedInTest);
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(30), DateTime.Today + TimeSpan.FromDays(30));

			var personRequest = new PersonRequestFactory().CreatePersonRequest().WithId();
			personRequest.Request.SetId(Guid.NewGuid());
			((IBelongsToBusinessUnit)personRequest).SetBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			PersonRequestRepository.Add(personRequest);
			Target.Handle(new PersonRequestChangedEvent
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			AnalyticsRequestRepository.AnalyticsRequestedDays.Should().Be.Empty();
			AnalyticsRequestRepository.AnalyticsRequests.Should().Be.Empty();
		}

		[Test]
		public void ShouldAddRequestAndRequestedDay()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.BusinessUnitUsedInTest);
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(30), DateTime.Today + TimeSpan.FromDays(30));

			var person = PersonFactory.CreatePerson("Test").WithId();
			var request = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1))).WithId();
			var personRequest = new PersonRequest(person, request).WithId();
			personRequest.SetBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			PersonRequestRepository.Add(personRequest);

			PersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonCode = person.Id.GetValueOrDefault(),
				PersonId = 1,
				ValidFromDate = DateTime.UtcNow - TimeSpan.FromDays(10),
				ValidToDate = DateTime.UtcNow + TimeSpan.FromDays(10)
			});

			Target.Handle(new PersonRequestDeletedEvent
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			AnalyticsRequestRepository.AnalyticsRequestedDays.Should().Not.Be.Empty();
			AnalyticsRequestRepository.AnalyticsRequests.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldAddRequestWithGivenBusinessUnit()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.BusinessUnitUsedInTest);
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(30), DateTime.Today + TimeSpan.FromDays(30));

			var person = PersonFactory.CreatePerson("Test").WithId();
			var request = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1))).WithId();
			var personRequest = new PersonRequest(person, request).WithId();
			personRequest.SetBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			PersonRequestRepository.Add(personRequest);

			var analyticsBu = new AnalyticBusinessUnit
			{
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				BusinessUnitId = 11
			};
			((FakeAnalyticsBusinessUnitRepository) AnalyticsBusinessUnitRepository).UseList = true;
			AnalyticsBusinessUnitRepository.AddOrUpdate(analyticsBu);

			PersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonCode = person.Id.GetValueOrDefault(),
				PersonId = 1,
				ValidFromDate = DateTime.UtcNow - TimeSpan.FromDays(10),
				ValidToDate = DateTime.UtcNow + TimeSpan.FromDays(10)
			});

			Target.Handle(new PersonRequestDeletedEvent
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			AnalyticsRequestRepository.AnalyticsRequestedDays.Should().Not.Be.Empty();
			AnalyticsRequestRepository.AnalyticsRequests.Should().Not.Be.Empty();
			AnalyticsRequestRepository.AnalyticsRequests[0].BusinessUnitId.Should().Be.EqualTo(analyticsBu.BusinessUnitId);
		}


		[Test]
		public void ShouldDeleteRequestAndRequestedDay()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.BusinessUnitUsedInTest);
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(30), DateTime.Today + TimeSpan.FromDays(30));

			var person = PersonFactory.CreatePerson("Test").WithId();
			var request = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1))).WithId();
			var personRequest = new PersonRequest(person, request).WithId();
			personRequest.SetDeleted();
			PersonRequestRepository.Add(personRequest);
			AnalyticsRequestRepository.AnalyticsRequestedDays.Add(new AnalyticsRequestedDay { RequestCode = personRequest.Id.GetValueOrDefault() });
			AnalyticsRequestRepository.AnalyticsRequests.Add(new AnalyticsRequest { RequestCode = personRequest.Id.GetValueOrDefault() });

			PersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonCode = person.Id.GetValueOrDefault(),
				PersonId = 1,
				ValidFromDate = DateTime.UtcNow - TimeSpan.FromDays(10),
				ValidToDate = DateTime.UtcNow + TimeSpan.FromDays(10)
			});

			Target.Handle(new PersonRequestChangedEvent
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			AnalyticsRequestRepository.AnalyticsRequestedDays.Should().Be.Empty();
			AnalyticsRequestRepository.AnalyticsRequests.Should().Be.Empty();
		}

		[Test]
		public void ShouldDeleteRequestPlacedAfterTerminationDate()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.BusinessUnitUsedInTest);
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(30), DateTime.Today + TimeSpan.FromDays(30));

			var person = PersonFactory.CreatePerson("Test").WithId();

			var requestedDate = DateTime.UtcNow + TimeSpan.FromDays(14);
			var request = new TextRequest(new DateTimePeriod(requestedDate, requestedDate)).WithId();
			var personRequest = new PersonRequest(person, request).WithId();
			
			PersonRequestRepository.Add(personRequest);
			AnalyticsRequestRepository.AnalyticsRequestedDays.Add(new AnalyticsRequestedDay { RequestCode = personRequest.Id.GetValueOrDefault() });
			AnalyticsRequestRepository.AnalyticsRequests.Add(new AnalyticsRequest { RequestCode = personRequest.Id.GetValueOrDefault() });

			PersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonCode = person.Id.GetValueOrDefault(),
				PersonId = 1,
				ValidFromDate = DateTime.UtcNow - TimeSpan.FromDays(10),
				ValidToDate = DateTime.UtcNow + TimeSpan.FromDays(10)
			});

			person.TerminatePerson(DateOnly.Today.AddDays(7), new PersonAccountUpdaterDummy());

			Target.Handle(new PersonRequestChangedEvent
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			AnalyticsRequestRepository.AnalyticsRequestedDays.Should().Be.Empty();
			AnalyticsRequestRepository.AnalyticsRequests.Should().Be.Empty();
		}

		[Test]
		public void ShouldAddOvertimeRequestAndSaveRequestTypeId()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.BusinessUnitUsedInTest);
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(30),
				DateTime.Today + TimeSpan.FromDays(30));

			var person = PersonFactory.CreatePerson("Test").WithId();
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("overtimepaid", MultiplicatorType.Overtime).WithId();
			var request = new OvertimeRequest(multiplicatorDefinitionSet,
				new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1))).WithId();
			var personRequest = new PersonRequest(person, request).WithId();
			personRequest.SetBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			PersonRequestRepository.Add(personRequest);

			PersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonCode = person.Id.GetValueOrDefault(),
				PersonId = 1,
				ValidFromDate = DateTime.UtcNow - TimeSpan.FromDays(10),
				ValidToDate = DateTime.UtcNow + TimeSpan.FromDays(10)
			});

			Target.Handle(new PersonRequestDeletedEvent
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			AnalyticsRequestRepository.AnalyticsRequests.Should().Not.Be.Empty();
			AnalyticsRequestRepository.AnalyticsRequests[0].RequestTypeId.Should().Be((int) RequestType.OvertimeRequest);
		}
	}
}