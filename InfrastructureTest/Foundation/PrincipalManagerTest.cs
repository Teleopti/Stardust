using System.Security.Principal;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    public class PrincipalManagerTest
    {
        private IIdentity threadPreviousIdentity;
        private IPerson threadPreviousPerson;
        private PrincipalManager target;

        [SetUp]
        public void Setup()
        {
            threadPreviousIdentity = Thread.CurrentPrincipal.Identity;
            threadPreviousPerson = ((IUnsafePerson)TeleoptiPrincipal.Current).Person;
            target = new PrincipalManager(new TeleoptiPrincipalFactory());
        }

        [Test]
        public void ShouldChangeCurrentPrincipalWhenTeleoptiPrincipalForThread()
        {
            IPerson newPerson = PersonFactory.CreatePerson();

            target.SetCurrentPrincipal(newPerson,null,null, AuthenticationTypeOption.Unknown);

            var currentPrincipal = TeleoptiPrincipal.Current;
            ((IUnsafePerson)currentPrincipal).Person.Should().Be.EqualTo(newPerson);
            currentPrincipal.Identity.Should().Not.Be.EqualTo(threadPreviousIdentity);
        }

        [Test]
        public void ShouldChangeCurrentPrincipalWhenNotTeleoptiPrincipalForThread()
        {
            IPerson newPerson = PersonFactory.CreatePerson();

            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("test"),new string[]{});
				target.SetCurrentPrincipal(newPerson, null, null, AuthenticationTypeOption.Unknown);

            var currentPrincipal = TeleoptiPrincipal.Current;
            ((IUnsafePerson)currentPrincipal).Person.Should().Be.EqualTo(newPerson);
            currentPrincipal.Identity.Should().Not.Be.EqualTo(threadPreviousIdentity);
        }

		 [Test]
		 public void ShouldSetAuthenticationType()
		 {
		 	const AuthenticationTypeOption authType = AuthenticationTypeOption.Application;

			 Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("test"), new string[] { });
			 target.SetCurrentPrincipal(null, null, null, authType);

			 var currentPrincipal = TeleoptiPrincipal.Current;
			 ((ITeleoptiIdentity)currentPrincipal.Identity).TeleoptiAuthenticationType.Should().Be.EqualTo(authType);
		 }

        [TearDown]
        public void Teardown()
        {
            Thread.CurrentPrincipal = new TeleoptiPrincipal(threadPreviousIdentity, threadPreviousPerson);
        }
    }
}
