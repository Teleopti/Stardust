using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	public class ThreadPrincipalContextTest
	{
		[Test]
		public void ShouldBeCovered()
		{
			var threadPreviousIdentity = Thread.CurrentPrincipal.Identity;
			var threadPreviousPerson = ((IUnsafePerson)TeleoptiPrincipal.Current).Person;
			try
			{
				var principal = MockRepository.GenerateMock<ITeleoptiPrincipal>();
				var factory = MockRepository.GenerateMock<IPrincipalFactory>();
				factory.Stub(x => x.MakePrincipal(null, null, null)).Return(principal);
				var target = new ThreadPrincipalContext(factory);

				target.SetCurrentPrincipal(null, null, null);

				Assert.That(Thread.CurrentPrincipal, Is.EqualTo(principal));
			}
			finally
			{
				Thread.CurrentPrincipal = new TeleoptiPrincipal(threadPreviousIdentity, threadPreviousPerson);
				((TeleoptiPrincipal)TeleoptiPrincipal.Current).ChangePrincipal(new TeleoptiPrincipal(threadPreviousIdentity, threadPreviousPerson));
			}
		}
	}
}