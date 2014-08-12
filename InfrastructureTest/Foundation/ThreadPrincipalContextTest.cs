using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture, Ignore]
	public class ThreadPrincipalContextTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldBeCovered()
		{
			var storedPrincipal = Thread.CurrentPrincipal;

			var principal = MockRepository.GenerateMock<ITeleoptiPrincipal>();
			var factory = MockRepository.GenerateMock<IPrincipalFactory>();
			factory.Stub(x => x.MakePrincipal(null, null, null)).Return(principal);
			var target = new ThreadPrincipalContext(factory);

			target.SetCurrentPrincipal(null, null, null);

			Assert.That(Thread.CurrentPrincipal, Is.EqualTo(principal));

			Thread.CurrentPrincipal = storedPrincipal;
		}

	}
}