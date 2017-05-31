using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class RunWaitlistCommandHandlerTest : ISetup
	{
		public RunWaitlistCommandHandler Target;
		public FakeQueuedAbsenceRequestRepository FakeQueuedAbsenceRequestRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<RunWaitlistCommandHandler>().For<IHandleCommand<RunWaitlistCommand>>();
		}

		[Test]
		public void ShouldAddPlaceholderForEachDayInPeriod()
		{
			var command = new RunWaitlistCommand
			{
				Period = new DateTimePeriod(2016, 12, 24, 12, 2016, 12, 31, 12)
			};

			Target.Handle(command);

			FakeQueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(8);
			FakeQueuedAbsenceRequestRepository.LoadAll().ForEach(x => x.PersonRequest.Should().Be.EqualTo(Guid.Empty));
		}
	}
}