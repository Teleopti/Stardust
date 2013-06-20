﻿using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;
using Teleopti.Interfaces.Messages.Requests;

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
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var currentBusinessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var currentDataSourceProvider = MockRepository.GenerateMock<ICurrentDataSource>();
			var now = MockRepository.GenerateMock<INow>();
			var time = new DateTime(2012, 05, 08, 12, 01, 01);

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

			now.Stub(x => x.LocalDateTime()).Return(time);

			mapper.Stub(x => x.Map<AbsenceRequestForm, IPersonRequest>(form)).Return(personRequest);

			var target = new AbsenceRequestPersister(personRequestRepository, mapper, serviceBusSender, currentBusinessUnitProvider, currentDataSourceProvider, now, null);
			target.Persist(form);

			personRequestRepository.AssertWasCalled(x => x.Add(personRequest));
		}

		[Test]
		public void ShouldSendMessageToBus()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			serviceBusSender.Stub(x => x.EnsureBus()).Return(true);
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

			mapper.Stub(x => x.Map<AbsenceRequestForm, IPersonRequest>(form)).Return(personRequest);

			var message = new NewAbsenceRequestCreated
			              	{
			              		BusinessUnitId = buId,
			              		Datasource = datasource.DataSourceName,
			              		PersonRequestId = personRequestId,
			              		Timestamp = time
			              	};

			var target = new AbsenceRequestPersister(personRequestRepository, mapper, serviceBusSender, currentBusinessUnitProvider, currentDataSourceProvider, now, currentUnitOfWork);
			target.Persist(form);

			currUow.Expect(c => c.AfterSuccessfulTx(() => serviceBusSender.Send(message)));
		}

		[Test]
		public void ShouldNotSendMessageToBrokenBus()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			serviceBusSender.Stub(x => x.EnsureBus()).Return(false);
			var currentBusinessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var currentDataSourceProvider = MockRepository.GenerateMock<ICurrentDataSource>();
			var now = MockRepository.GenerateMock<INow>();
			var time = new DateTime(2012, 05, 08, 12, 01, 01);
			now.Stub(x => x.UtcDateTime()).Return(time);

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

			var target = new AbsenceRequestPersister(personRequestRepository, mapper, serviceBusSender, currentBusinessUnitProvider, currentDataSourceProvider, now, null);
			target.Persist(form);

			serviceBusSender.AssertWasNotCalled(x => x.Send(Arg<RaptorDomainMessage>.Is.Anything));
			personRequest.AssertWasCalled(x => x.Pending());
		}

		[Test]
		public void ShouldUpdateExistingAbsenceRequest()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var currentBusinessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var currentDataSourceProvider = MockRepository.GenerateMock<ICurrentDataSource>();
			var now = MockRepository.GenerateMock<INow>();

			var form = new AbsenceRequestForm();
			var personRequestId = Guid.NewGuid();
			form.EntityId = personRequestId;
			personRequestRepository.Stub(x => x.Find(personRequestId)).Return(personRequest);

			mapper.Stub(x => x.Map<AbsenceRequestForm, IPersonRequest>(form)).Return(personRequest);

			var target = new AbsenceRequestPersister(personRequestRepository, mapper, serviceBusSender, currentBusinessUnitProvider, currentDataSourceProvider, now, null);
			target.Persist(form);

			personRequestRepository.AssertWasNotCalled(x => x.Add(personRequest));
			mapper.AssertWasCalled(x => x.Map(form, personRequest));
		}
	}
}