using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class ShiftTradeRequestPersisterTest
	{
		private IShiftTradeRequestMapper mapper;
		private IMappingEngine autoMapper;
		private IPersonRequestRepository repository;
		private IServiceBusEventPublisher serviceBusSender;
		private IShiftTradeRequestSetChecksum shiftTradeSetChecksum;

		[SetUp]
		public void Setup()
		{
			mapper = MockRepository.GenerateMock<IShiftTradeRequestMapper>();
			autoMapper = MockRepository.GenerateMock<IMappingEngine>();
			repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			serviceBusSender = MockRepository.GenerateMock<IServiceBusEventPublisher>();
			shiftTradeSetChecksum = MockRepository.GenerateMock<IShiftTradeRequestSetChecksum>();
		}

		[Test]
		public void ShouldPersistMappedData()
		{
			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, serviceBusSender, null, null, null, null, shiftTradeSetChecksum);
			var form = new ShiftTradeRequestForm();
			var shiftTradeRequest = new PersonRequest(new Person());
			var viewModel = new RequestViewModel();

			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);
			autoMapper.Stub(x => x.Map<IPersonRequest, RequestViewModel>(shiftTradeRequest)).Return(viewModel);
			serviceBusSender.Expect(x => x.EnsureBus()).Return(false);

			var result = target.Persist(form);

			result.Should().Be.SameInstanceAs(viewModel);
			repository.AssertWasCalled(x => x.Add(shiftTradeRequest));
		}

		[Test]
		public void ShouldSendMessageToBus()
		{
			//bajstest - calling bus shouldn't happen here at all I think...
			//therefore - just dummy test
			var now = MockRepository.GenerateMock<INow>();
			now.Expect(x => x.UtcDateTime()).Return(DateTime.Now);
			var dataSourceProvider = MockRepository.GenerateMock<ICurrentDataSource>();
			dataSourceProvider.Expect(x => x.Current()).Return(MockRepository.GenerateMock<IDataSource>());
			var businessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			businessUnitProvider.Expect(x => x.Current()).Return(new BusinessUnit("d"));
			var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			var form = new ShiftTradeRequestForm();
			mapper.Stub(x => x.Map(form)).Return(new PersonRequest(new Person()));
			var target = new ShiftTradeRequestPersister(MockRepository.GenerateMock<IPersonRequestRepository>(),
			                                            mapper,
			                                            autoMapper,
			                                            serviceBusSender,
			                                            now,
			                                            dataSourceProvider,
			                                            businessUnitProvider,
			                                            currentUnitOfWork,
																									shiftTradeSetChecksum);
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			currentUnitOfWork.Expect(x => x.Current()).Return(uow);
			serviceBusSender.Expect(x => x.EnsureBus()).Return(true);

			target.Persist(form);

			uow.AssertWasCalled(x => x.AfterSuccessfulTx(Arg<Action>.Is.Anything));
		}

		[Test]
		public void ShouldNotSendMessageToBus()
		{
			//bajstest - calling bus shouldn't happen here at all I think...
			//therefore - just dummy test
			var now = MockRepository.GenerateMock<INow>();
			now.Expect(x => x.UtcDateTime()).Return(DateTime.Now);
			var dataSourceProvider = MockRepository.GenerateMock<ICurrentDataSource>();
			var businessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			var form = new ShiftTradeRequestForm();
			mapper.Stub(x => x.Map(form)).Return(new PersonRequest(new Person()));
			var target = new ShiftTradeRequestPersister(MockRepository.GenerateMock<IPersonRequestRepository>(),
																									mapper,
																									autoMapper,
																									serviceBusSender,
																									now,
																									dataSourceProvider,
																									businessUnitProvider,
																									currentUnitOfWork,
																									shiftTradeSetChecksum);
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			currentUnitOfWork.Expect(x => x.Current()).Return(uow);
			serviceBusSender.Expect(x => x.EnsureBus()).Return(false);

			target.Persist(form);

			uow.AssertWasNotCalled(x => x.AfterSuccessfulTx(Arg<Action>.Is.Anything));
		}

		[Test]
		public void ShouldSetChecksumOnRequest()
		{
			//elände - borde inte behöva anropa setchecksum explicit
			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, serviceBusSender, null, null, null, null, shiftTradeSetChecksum);
			var form = new ShiftTradeRequestForm();
			var shiftTradeRequest = new PersonRequest(new Person());

			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);
			serviceBusSender.Expect(x => x.EnsureBus()).Return(false);

			target.Persist(form);

			shiftTradeSetChecksum.AssertWasCalled(x => x.SetChecksum(shiftTradeRequest.Request));
		}

	}
}