using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	class AbsenceRequestPersisterTest
	{
		[Test]
		public void ShouldAddTextRequest()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var currentBusinessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnitProvider>();
			var currentDataSourceProvider = MockRepository.GenerateMock<ICurrentDataSourceProvider>();
			var now = MockRepository.GenerateMock<INow>();
			var time = new DateTime(2012, 05, 08, 12, 01, 01);

			var form = new AbsenceRequestForm();

			var bu = MockRepository.GenerateMock<IBusinessUnit>();
			var buId = Guid.NewGuid();
			bu.Stub(x => x.Id).Return(buId);
			currentBusinessUnitProvider.Stub(x => x.CurrentBusinessUnit()).Return(bu);

			var personRequestId = Guid.NewGuid();
			personRequest.Stub(x => x.Id).Return(personRequestId);

			var datasource = MockRepository.GenerateMock<IDataSource>();
			datasource.Stub(x => x.DataSourceName).Return("Data Source");
			currentDataSourceProvider.Stub(x => x.CurrentDataSource()).Return(datasource);

			now.Stub(x => x.Time).Return(time);

			mapper.Stub(x => x.Map<AbsenceRequestForm, IPersonRequest>(form)).Return(personRequest);

			var target = new AbsenceRequestPersister(personRequestRepository, mapper, serviceBusSender, currentBusinessUnitProvider, currentDataSourceProvider, now);
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
			var currentBusinessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnitProvider>();
			var currentDataSourceProvider = MockRepository.GenerateMock<ICurrentDataSourceProvider>();
			var now = MockRepository.GenerateMock<INow>();
			var time = new DateTime(2012, 05, 08, 12, 01, 01);
			now.Stub(x => x.UtcTime).Return(time);

			var form = new AbsenceRequestForm();

			var bu = MockRepository.GenerateMock<IBusinessUnit>();
			var buId = Guid.NewGuid();
			bu.Stub(x => x.Id).Return(buId);
			currentBusinessUnitProvider.Stub(x => x.CurrentBusinessUnit()).Return(bu);

			var personRequestId = Guid.NewGuid();
			personRequest.Stub(x => x.Id).Return(personRequestId);

			var datasource = MockRepository.GenerateMock<IDataSource>();
			datasource.Stub(x => x.DataSourceName).Return("Data Source");
			currentDataSourceProvider.Stub(x => x.CurrentDataSource()).Return(datasource);

			mapper.Stub(x => x.Map<AbsenceRequestForm, IPersonRequest>(form)).Return(personRequest);

			var message = new NewAbsenceRequestCreated()
			{
				BusinessUnitId = buId,
				Datasource = datasource.DataSourceName,
				PersonRequestId = personRequestId,
				Timestamp = time
			};

			var target = new AbsenceRequestPersister(personRequestRepository, mapper, serviceBusSender, currentBusinessUnitProvider, currentDataSourceProvider, now);
			target.Persist(form);

			var usedMessage = (NewAbsenceRequestCreated)serviceBusSender.GetArgumentsForCallsMadeOn(x => x.NotifyServiceBus(message))[0][0];
			usedMessage.BusinessUnitId.Should().Be.EqualTo(buId);
			usedMessage.Datasource.Should().Be.EqualTo(datasource.DataSourceName);
			usedMessage.PersonRequestId.Should().Be.EqualTo(personRequestId);
			usedMessage.Timestamp.Should().Be.EqualTo(time);
		}

		[Test]
		public void ShouldNotSendMessageToBrokenBus()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			serviceBusSender.Stub(x => x.EnsureBus()).Return(false);
			var currentBusinessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnitProvider>();
			var currentDataSourceProvider = MockRepository.GenerateMock<ICurrentDataSourceProvider>();
			var now = MockRepository.GenerateMock<INow>();
			var time = new DateTime(2012, 05, 08, 12, 01, 01);
			now.Stub(x => x.UtcTime).Return(time);

			var form = new AbsenceRequestForm();

			var bu = MockRepository.GenerateMock<IBusinessUnit>();
			var buId = Guid.NewGuid();
			bu.Stub(x => x.Id).Return(buId);
			currentBusinessUnitProvider.Stub(x => x.CurrentBusinessUnit()).Return(bu);

			var personRequestId = Guid.NewGuid();
			personRequest.Stub(x => x.Id).Return(personRequestId);

			var datasource = MockRepository.GenerateMock<IDataSource>();
			datasource.Stub(x => x.DataSourceName).Return("Data Source");
			currentDataSourceProvider.Stub(x => x.CurrentDataSource()).Return(datasource);

			mapper.Stub(x => x.Map<AbsenceRequestForm, IPersonRequest>(form)).Return(personRequest);

			var target = new AbsenceRequestPersister(personRequestRepository, mapper, serviceBusSender, currentBusinessUnitProvider, currentDataSourceProvider, now);
			target.Persist(form);

			serviceBusSender.AssertWasNotCalled(x => x.NotifyServiceBus(Arg<RaptorDomainMessage>.Is.Anything));
			personRequest.AssertWasCalled(x => x.Pending());
		}
	}
}
