using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
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
		private IMessagePopulatingServiceBusSender serviceBusSender;
		private IShiftTradeRequestSetChecksum shiftTradeSetChecksum;
		private IShiftTradeRequestProvider shiftTradeRequestProvider;

		[SetUp]
		public void Setup()
		{
			mapper = MockRepository.GenerateMock<IShiftTradeRequestMapper>();
			autoMapper = MockRepository.GenerateMock<IMappingEngine>();
			repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			serviceBusSender = MockRepository.GenerateMock<IMessagePopulatingServiceBusSender>();
			shiftTradeSetChecksum = MockRepository.GenerateMock<IShiftTradeRequestSetChecksum>();
			shiftTradeRequestProvider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
		}

		[Test]
		public void ShouldPersistMappedData()
		{
			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, serviceBusSender, null, null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider);
			var form = new ShiftTradeRequestForm();
			var shiftTradeRequest = new PersonRequest(new Person());
			var viewModel = new RequestViewModel();

			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);
			autoMapper.Stub(x => x.Map<IPersonRequest, RequestViewModel>(shiftTradeRequest)).Return(viewModel);

			var result = target.Persist(form);

			result.Should().Be.SameInstanceAs(viewModel);
			repository.AssertWasCalled(x => x.Add(shiftTradeRequest));
		}		
		
		[Test]
		public void ShouldAutoApprovedByAnnouncerWhenShiftTradeFromBulletinBoard()
		{
			shiftTradeRequestProvider.Stub(x => x.RetrieveUserWorkflowControlSet())
				.Return(new WorkflowControlSet("bla") {LockTrading = true, AutoGrantShiftTradeRequest = false});
			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, serviceBusSender, null, null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider);
			var form = new ShiftTradeRequestForm(){ShiftExchangeOfferId = new Guid()};
			var shiftTradeRequest = new PersonRequest(new Person());
			var viewModel = new RequestViewModel();

			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);
			autoMapper.Stub(x => x.Map<IPersonRequest, RequestViewModel>(shiftTradeRequest)).Return(viewModel);

			var result = target.Persist(form);

			result.Status.Should().Be.EqualTo(Resources.WaitingThreeDots);
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
																									shiftTradeSetChecksum, shiftTradeRequestProvider);
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			currentUnitOfWork.Expect(x => x.Current()).Return(uow);

			target.Persist(form);

			uow.AssertWasCalled(x => x.AfterSuccessfulTx(Arg<Action>.Is.Anything));
		}		

		[Test]
		public void ShouldSetChecksumOnRequest()
		{
			//elände - borde inte behöva anropa setchecksum explicit
			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, serviceBusSender, null, null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider);
			var form = new ShiftTradeRequestForm();
			var shiftTradeRequest = new PersonRequest(new Person());

			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			target.Persist(form);

			shiftTradeSetChecksum.AssertWasCalled(x => x.SetChecksum(shiftTradeRequest.Request));
		}

	}
}