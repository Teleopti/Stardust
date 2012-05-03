using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class DenormalizationQueueRepositoryTest : DatabaseTest
	{
		private IDenormalizerQueueRepository target;

		[Test]
		public void ShouldDequeueItems()
		{
			target = new DenormalizerQueueRepository(UnitOfWorkFactory.Current);
			target.DequeueDenormalizerMessages(((ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity).BusinessUnit).Should().Be.
				Empty();
		}
	}
}