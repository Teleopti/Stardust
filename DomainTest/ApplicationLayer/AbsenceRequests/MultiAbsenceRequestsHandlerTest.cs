using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class MultiAbsenceRequestsHandlerTest : ISetup
	{
		public IPersonRequestRepository PersonRequestRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public IStardustJobFeedback Feedback;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IMultiAbsenceRequestsUpdater MultiAbsenceRequestsUpdater;
		public IAbsenceRequestValidatorProvider AbsenceRequestValidatorProvider;
		public MultiAbsenceRequestsHandler Target;
		public FakeDataSourceForTenant DataSourceForTenant;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeCurrentTeleoptiPrincipal CurrentTeleoptiPrincipal;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;

		private Domain.Common.BusinessUnit _businessUnit;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCurrentTeleoptiPrincipal>().For<ICurrentTeleoptiPrincipal>();
			system.UseTestDouble(MockRepository.GenerateMock<IAbsenceRequestValidatorProvider>())
				.For<IAbsenceRequestValidatorProvider>();
		}

		[Test]
		public void ShouldHandleRequestWithSpecificValidators()
		{
			setDataForTest();

			var person = createPerson();
			var personRequest = createPersonRequest(person);

			addToQueue(personRequest, RequestValidatorsFlag.IntradayValidator);

			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid> {personRequest.Id.GetValueOrDefault()},
				InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
				LogOnBusinessUnitId = _businessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				Sent = DateTime.UtcNow
			});

			Assert.IsTrue(personRequest.IsPending);
			Assert.IsTrue(personRequest.GetMessage(new NoFormatting()) == "validatorErrors1");
		}

		[Test]
		public void ShouldHandleRequestsWithDifferentSpecificValidators()
		{
			setDataForTest();

			var person = createPerson();
			var personRequest1 = createPersonRequest(person);
			var personRequest2 = createPersonRequest(person);

			addToQueue(personRequest1, RequestValidatorsFlag.IntradayValidator);
			addToQueue(personRequest2, RequestValidatorsFlag.BudgetAllotmentValidator);

			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid> { personRequest1.Id.GetValueOrDefault(), personRequest2.Id.GetValueOrDefault() },
				InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
				LogOnBusinessUnitId = _businessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				Sent = DateTime.UtcNow
			});

			Assert.IsTrue(personRequest1.IsPending);
			Assert.IsTrue(personRequest1.GetMessage(new NoFormatting()) == "validatorErrors1");

			Assert.IsTrue(personRequest2.IsPending);
			Assert.IsTrue(personRequest2.GetMessage(new NoFormatting()) == "validatorErrors2");
		}

		private void setDataForTest()
		{
			setBusinessUnit();
			setCurrentScenario();
			setAbsenceRequestValidatorProvider();
			setDataSource();
			setCurrentPrincipal();
		}

		private void setBusinessUnit()
		{
			_businessUnit = new Domain.Common.BusinessUnit("test bu").WithId();
			BusinessUnitRepository.Add(_businessUnit);
		}

		private void setCurrentScenario()
		{
			var scenario = MockRepository.GenerateMock<Scenario>();
			scenario.Stub(s => s.DefaultScenario).Return(true);
			scenario.Stub(s => s.BusinessUnit).Return(_businessUnit);
			scenario.Stub(s => s.Id).Return(Guid.NewGuid());
			scenario.Stub(s => s.Equals(scenario)).Return(true);
			ScenarioRepository.Has(scenario);
		}

		private void setAbsenceRequestValidatorProvider()
		{
			var absenceRequestValidator1 = MockRepository.GenerateMock<IAbsenceRequestValidator>();
			absenceRequestValidator1.Stub(a => a.Validate(null, new RequiredForHandlingAbsenceRequest()))
				.IgnoreArguments().Return(new ValidatedRequest { IsValid = false, ValidationErrors = "validatorErrors1" });

			var absenceRequestValidator2 = MockRepository.GenerateMock<IAbsenceRequestValidator>();
			absenceRequestValidator2.Stub(a => a.Validate(null, new RequiredForHandlingAbsenceRequest()))
				.IgnoreArguments().Return(new ValidatedRequest { IsValid = false, ValidationErrors = "validatorErrors2" });

			AbsenceRequestValidatorProvider.Stub(a => a.GetValidatorList(RequestValidatorsFlag.IntradayValidator))
				.Return(new[] { absenceRequestValidator1 });
			AbsenceRequestValidatorProvider.Stub(a => a.GetValidatorList(RequestValidatorsFlag.BudgetAllotmentValidator))
				.Return(new[] { absenceRequestValidator2 });
		}

		private void setDataSource()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			dataSource.Stub(d => d.DataSourceName).Return("Teleopti WFM");
			dataSource.Stub(d => d.Application).Return(new FakeUnitOfWorkFactory());
			DataSourceForTenant.Has(dataSource);
		}

		private void setCurrentPrincipal()
		{
			var person = createPerson();
			var identity = new TeleoptiIdentity("testPerson", null, null, null, null);
			CurrentTeleoptiPrincipal.SetCurrentPrincipal(new TeleoptiPrincipal(identity, person));
		}

		private IPerson createPerson()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.WorkflowControlSet = new WorkflowControlSet();
			PersonRepository.Add(person);
			return person;
		}

		private IPersonRequest createPersonRequest(IPerson person)
		{
			var absence = AbsenceFactory.CreateAbsence("holiday");
			var personRequestFactory = new PersonRequestFactory();
			var personRequest = personRequestFactory.CreatePersonRequest(person).WithId();
			var absenceRequest = personRequestFactory.CreateAbsenceRequest(absence,
				new DateTimePeriod(DateTime.Now.AddDays(1).Date.ToUniversalTime(), DateTime.Now.AddDays(2).Date.ToUniversalTime()))
				.WithId();
			personRequest.Request = absenceRequest;
			absenceRequest.SetParent(personRequest);
			PersonRequestRepository.Add(personRequest);
			return personRequest;
		}

		private void addToQueue(IPersonRequest personRequest, RequestValidatorsFlag requestValidatorsFlag)
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				PersonRequest = personRequest.Id.GetValueOrDefault(),
				MandatoryValidators = requestValidatorsFlag
			});
		}
	}
}
