using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class EtlLogObjectRepositoryTest : DatabaseTestWithoutTransaction
	{
		[Test]
		public void ShouldLoadLogObjectDetails()
		{
			var target = new EtlLogObjectRepository(new CurrentDataSource(new CurrentIdentity(new CurrentTeleoptiPrincipal())));
			var model = target.Load();
			Assert.That(model, Is.Not.Null);
		}
	}
}