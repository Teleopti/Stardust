using System;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class QuickForecastCommandHandlerTest
	{
		private MockRepository _mocks;
		private  ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private  IJobResultRepository _jobResultRepository;
		private QuickForecastCommandHandler _target;
		private IUnitOfWork _unitOfWork;
	    private LegacyFakeEventPublisher _publisher;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			_jobResultRepository = _mocks.DynamicMock<IJobResultRepository>();
		    _publisher = new LegacyFakeEventPublisher();
            _target = new QuickForecastCommandHandler( _unitOfWorkFactory, _jobResultRepository, _publisher, new DummyInfrastructureInfoPopulator(), MockRepository.GenerateMock<ILoggedOnUser>());
			_unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
		}

		[Test]
		public void ShouldThrowFaultExceptionIfServiceBusIsNotAvailable()
		{
			Expect.Call(_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(() => _jobResultRepository.Add(null)).IgnoreArguments();
			Expect.Call(() => _unitOfWork.PersistAll());
			Expect.Call(_unitOfWork.Dispose);
			
			_mocks.ReplayAll();
			_target.Handle(new QuickForecastCommandDto
				{
					TargetPeriod =
						new DateOnlyPeriodDto
							{
								StartDate = new DateOnlyDto {DateTime = DateTime.Today},
								EndDate = new DateOnlyDto {DateTime = DateTime.Today}
							}
				});
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldThrowFaultExceptionIfCommandIsNull()
		{
			Assert.Throws<FaultException>(() => _target.Handle(null));
		}

		[Test]
		public void ShouldSendToServiceBus()
		{
			Expect.Call(_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(() => _jobResultRepository.Add(null)).IgnoreArguments();
			Expect.Call(() => _unitOfWork.PersistAll());
			Expect.Call(_unitOfWork.Dispose);
			Expect.Call(() => _publisher.Publish(new QuickForecastWorkloadsEvent())).IgnoreArguments();
			_mocks.ReplayAll();
			var period = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto {DateTime = DateTime.Today},
					EndDate = new DateOnlyDto {DateTime = DateTime.Today}
				};
			_target.Handle(new QuickForecastCommandDto
			{
				TargetPeriod = period,
				TemplatePeriod = period,
				StatisticPeriod = period,
				ScenarioId = Guid.NewGuid(),
				WorkloadIds = new List<Guid>()
					
			});
			_mocks.VerifyAll();
		}
	}

	
}