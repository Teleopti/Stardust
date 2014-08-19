using System.Security.Principal;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	public class WindowsAppDomainPrincipalContextTest
	{
		[Test]
		public void ShouldChangeCurrentPrincipalWhenTeleoptiPrincipalForThread()
		{
			var threadPreviousIdentity = Thread.CurrentPrincipal.Identity;
			var threadPreviousPerson = ((IUnsafePerson)TeleoptiPrincipal.Current).Person;
			try
			{
				var target = new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory());

				var newPerson = PersonFactory.CreatePerson();

				target.SetCurrentPrincipal(newPerson, null, null);

				var currentPrincipal = TeleoptiPrincipal.Current;
				((IUnsafePerson)currentPrincipal).Person.Should().Be.EqualTo(newPerson);
				currentPrincipal.Identity.Should().Not.Be.EqualTo(threadPreviousIdentity);
			}
			finally
			{
				((TeleoptiPrincipal)TeleoptiPrincipal.Current).ChangePrincipal(new TeleoptiPrincipal(threadPreviousIdentity, threadPreviousPerson));
			}
		}

		[Test]
		public void ShouldChangeCurrentPrincipalWhenNotTeleoptiPrincipalForThread()
		{
			var threadPreviousIdentity = Thread.CurrentPrincipal.Identity;
			var threadPreviousPerson = ((IUnsafePerson)TeleoptiPrincipal.Current).Person;

			try
			{
				var target = new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory());

				var newPerson = PersonFactory.CreatePerson();

				Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("test"), new string[] { });
				target.SetCurrentPrincipal(newPerson, null, null);

				var currentPrincipal = TeleoptiPrincipal.Current;
				((IUnsafePerson)currentPrincipal).Person.Should().Be.EqualTo(newPerson);
				currentPrincipal.Identity.Should().Not.Be.EqualTo(threadPreviousIdentity);
			}
			finally
			{
				((TeleoptiPrincipal)TeleoptiPrincipal.Current).ChangePrincipal(new TeleoptiPrincipal(threadPreviousIdentity, threadPreviousPerson));
			}
		}
	}
}
