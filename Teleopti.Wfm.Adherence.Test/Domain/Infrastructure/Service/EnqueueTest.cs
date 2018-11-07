using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;
using Teleopti.Wfm.Adherence.Test.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.Domain.Infrastructure.Service
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class EnqueueTest
	{
		public IPrincipalAndStateContext Context;
		public Rta Target;
		public IStateQueueReader Queue;
		public WithAnalyticsUnitOfWork UnitOfWork;

		[Test]
		public void ShouldEnqueue()
		{
			Context.Logout();

			Target.Enqueue(new BatchForTest());

			Context.Login();
			UnitOfWork.Get(() => Queue.Count())
				.Should().Be(1);
		}
	}
}