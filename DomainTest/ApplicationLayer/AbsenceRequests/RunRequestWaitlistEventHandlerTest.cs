using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[TestFixture]
	public class RunRequestWaitlistEventHandlerTest
	{
		private IWorkflowControlSet wcs1;
		private IWorkflowControlSet wcs2;
		private IUnitOfWork uow;
		private IAbsenceRequestWaitlistProcessor processor;
		private RunRequestWaitlistEventHandler target;
		private IWorkflowControlSetRepository wcsRepository;

		[SetUp]
		public void Setup()
		{
			wcs1 = new WorkflowControlSet();
			wcs2 = new WorkflowControlSet();
			wcsRepository = MockRepository.GenerateMock<IWorkflowControlSetRepository>();
			wcsRepository.Stub(x => x.LoadAll()).Return(new[] {wcs1, wcs2});

			uow = new FakeUnitOfWork();
			var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			currentUnitOfWork.Stub(x => x.Current()).Return(uow);

			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);

			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);

			processor = MockRepository.GenerateMock<IAbsenceRequestWaitlistProcessor>();
			target = new RunRequestWaitlistEventHandler(currentUnitOfWorkFactory, processor, wcsRepository);
		}

		[Test]
		public void ShouldTriggerWaitlistProcessing()
		{
			var period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
			target.Handle(new RunRequestWaitlistEvent
			{
				StartTime = period.StartDateTime,
				EndTime = period.EndDateTime
			});
			processor.AssertWasCalled(x => x.ProcessAbsenceRequestWaitlist(uow, period, wcs1));
			processor.AssertWasCalled(x => x.ProcessAbsenceRequestWaitlist(uow, period, wcs2));
		}
	}
}