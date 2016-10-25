﻿using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class AbsenceRequestPersisterTest
	{
		[Test]
		public void ShouldAddAbsenceRequest()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var person = MockRepository.GenerateMock<IPerson>();
			var workflowControlSet = MockRepository.GenerateMock<IWorkflowControlSet>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var absenceRequest = MockRepository.GenerateMock<IAbsenceRequest>();
			personRequest.Stub(x => x.Request).Return(absenceRequest);
			personRequest.Stub(x => x.Person).Return(person);
			person.Stub(x => x.WorkflowControlSet).Return(workflowControlSet);
			absenceRequest.Stub(x => x.Person).Return(person);
			var serviceBusSender = MockRepository.GenerateMock<IEventPublisher>();
			var currentBusinessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var currentDataSourceProvider = MockRepository.GenerateMock<ICurrentDataSource>();

			var form = new AbsenceRequestForm();

			var bu = MockRepository.GenerateMock<IBusinessUnit>();
			var buId = Guid.NewGuid();
			bu.Stub(x => x.Id).Return(buId);
			currentBusinessUnitProvider.Stub(x => x.Current()).Return(bu);

			var personRequestId = Guid.NewGuid();
			personRequest.Stub(x => x.Id).Return(personRequestId);

			var datasource = MockRepository.GenerateMock<IDataSource>();
			datasource.Stub(x => x.DataSourceName).Return("Data Source");
			currentDataSourceProvider.Stub(x => x.Current()).Return(datasource);

			mapper.Stub(x => x.Map<AbsenceRequestForm, IPersonRequest>(form)).Return(personRequest);

			var absenceRequestSynchronousValidator = MockRepository.GenerateMock<IAbsenceRequestSynchronousValidator>();
			absenceRequestSynchronousValidator.Stub(x => x.Validate(null)).IgnoreArguments().Return(new ValidatedRequest { IsValid = true });
			var target = new AbsenceRequestPersister(personRequestRepository, mapper, serviceBusSender, currentBusinessUnitProvider, currentDataSourceProvider, new Now(), null,
				absenceRequestSynchronousValidator, new PersonRequestAuthorizationCheckerForTest(), new AbsenceRequestIntradayFilterEmpty());
			target.Persist(form);

			personRequestRepository.AssertWasCalled(x => x.Add(personRequest));
		}

		[Test]
		public void ShouldSendMessage()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var eventSender = MockRepository.GenerateMock<IEventPublisher>();
			var currentBusinessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var currentDataSourceProvider = MockRepository.GenerateMock<ICurrentDataSource>();
			var now = MockRepository.GenerateMock<INow>();
			var time = new DateTime(2012, 05, 08, 12, 01, 01);
			now.Stub(x => x.UtcDateTime()).Return(time);
			var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			var currUow = MockRepository.GenerateMock<IUnitOfWork>();
			currentUnitOfWork.Expect(c => c.Current()).Return(currUow);

			var form = new AbsenceRequestForm();

			var bu = MockRepository.GenerateMock<IBusinessUnit>();
			var buId = Guid.NewGuid();
			bu.Stub(x => x.Id).Return(buId);
			currentBusinessUnitProvider.Stub(x => x.Current()).Return(bu);

			var personRequestId = Guid.NewGuid();
			personRequest.Stub(x => x.Id).Return(personRequestId);

			var datasource = MockRepository.GenerateMock<IDataSource>();
			datasource.Stub(x => x.DataSourceName).Return("Data Source");
			currentDataSourceProvider.Stub(x => x.Current()).Return(datasource);
			personRequest.Stub(p => p.Person).Return(new Person());
			mapper.Stub(x => x.Map<AbsenceRequestForm, IPersonRequest>(form)).Return(personRequest);

			var message = new NewAbsenceRequestCreatedEvent
			              	{
			              		LogOnBusinessUnitId = buId,
			              		LogOnDatasource = datasource.DataSourceName,
			              		PersonRequestId = personRequestId,
			              		Timestamp = time
			              	};

			var absenceRequestSynchronousValidator = MockRepository.GenerateMock<IAbsenceRequestSynchronousValidator>();
			absenceRequestSynchronousValidator.Stub(x => x.Validate(null)).IgnoreArguments().Return(new ValidatedRequest { IsValid = true });
			var target = new AbsenceRequestPersister(personRequestRepository, mapper, eventSender, currentBusinessUnitProvider, currentDataSourceProvider, now, currentUnitOfWork,
				absenceRequestSynchronousValidator, new PersonRequestAuthorizationCheckerForTest(), new AbsenceRequestIntradayFilterEmpty());
			target.Persist(form);

			currUow.Expect(c => c.AfterSuccessfulTx(() => eventSender.Publish(message)));
		}

		[Test]
		public void ShouldUpdateExistingAbsenceRequest()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var serviceBusSender = MockRepository.GenerateMock<IEventPublisher>();
			var currentBusinessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var currentDataSourceProvider = MockRepository.GenerateMock<ICurrentDataSource>();
			var now = MockRepository.GenerateMock<INow>();

			var form = new AbsenceRequestForm();
			var personRequestId = Guid.NewGuid();
			form.EntityId = personRequestId;
			personRequestRepository.Stub(x => x.Find(personRequestId)).Return(personRequest);

			mapper.Stub(x => x.Map<AbsenceRequestForm, IPersonRequest>(form)).Return(personRequest);

			var absenceRequestPersonAccountValidator = new AbsenceRequestPersonAccountValidator(
				new PersonAbsenceAccountProvider(new FakePersonAbsenceAccountRepository()));
			var absenceRequestSynchronousValidator = new AbsenceRequestSynchronousValidator(new ExpiredRequestValidator(new FakeGlobalSettingDataRepository(), new Now()),
				new AlreadyAbsentValidator(), new FakeScheduleDataReadScheduleStorage(), new FakeCurrentScenario(), new AbsenceRequestWorkflowControlSetValidator(), absenceRequestPersonAccountValidator);
			var target = new AbsenceRequestPersister(personRequestRepository, mapper, serviceBusSender, currentBusinessUnitProvider, currentDataSourceProvider, now, null,
				absenceRequestSynchronousValidator, new PersonRequestAuthorizationCheckerForTest(), new AbsenceRequestIntradayFilterEmpty());
			target.Persist(form);

			personRequestRepository.AssertWasNotCalled(x => x.Add(personRequest));
			mapper.AssertWasCalled(x => x.Map(form, personRequest));
		}
	}
}