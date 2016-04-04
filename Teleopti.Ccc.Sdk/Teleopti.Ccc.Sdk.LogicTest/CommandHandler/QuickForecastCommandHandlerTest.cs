using System;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class QuickForecastCommandHandlerTest
	{
		private MockRepository _mocks;
		private IMessagePopulatingServiceBusSender _busSender;
		private  ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private  IJobResultRepository _jobResultRepository;
		private QuickForecastCommandHandler _target;
		private IUnitOfWork _unitOfWork;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_busSender = _mocks.DynamicMock<IMessagePopulatingServiceBusSender>();
			_unitOfWorkFactory = _mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			_jobResultRepository = _mocks.DynamicMock<IJobResultRepository>();
			_target = new QuickForecastCommandHandler(_busSender, _unitOfWorkFactory, _jobResultRepository);
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
		[ExpectedException(typeof(FaultException))]
		public void ShouldThrowFaultExceptionIfCommandIsNull()
		{
			_target.Handle(null);
		}

		[Test]
		public void ShouldSendToServiceBus()
		{
			Expect.Call(_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(() => _jobResultRepository.Add(null)).IgnoreArguments();
			Expect.Call(() => _unitOfWork.PersistAll());
			Expect.Call(_unitOfWork.Dispose);
			Expect.Call(() => _busSender.Send(new QuickForecastWorkloadsMessage(), true)).IgnoreArguments();
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