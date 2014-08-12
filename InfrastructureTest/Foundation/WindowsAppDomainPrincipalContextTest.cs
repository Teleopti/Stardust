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
    [TestFixture, Ignore("Robin - can you have a look please?")]
    public class WindowsAppDomainPrincipalContextTest
    {
        private IIdentity threadPreviousIdentity;
        private IPerson threadPreviousPerson;
        private WindowsAppDomainPrincipalContext target;

        [SetUp]
        public void Setup()
        {
            threadPreviousIdentity = Thread.CurrentPrincipal.Identity;
            threadPreviousPerson = ((IUnsafePerson)TeleoptiPrincipal.Current).Person;
            target = new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory());
        }

        [Test]
        public void ShouldChangeCurrentPrincipalWhenTeleoptiPrincipalForThread()
        {
            IPerson newPerson = PersonFactory.CreatePerson();

            target.SetCurrentPrincipal(newPerson,null,null);

            var currentPrincipal = TeleoptiPrincipal.Current;
            ((IUnsafePerson)currentPrincipal).Person.Should().Be.EqualTo(newPerson);
            currentPrincipal.Identity.Should().Not.Be.EqualTo(threadPreviousIdentity);
        }

        [Test]
        public void ShouldChangeCurrentPrincipalWhenNotTeleoptiPrincipalForThread()
        {
            IPerson newPerson = PersonFactory.CreatePerson();

            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("test"),new string[]{});
				target.SetCurrentPrincipal(newPerson, null, null);

            var currentPrincipal = TeleoptiPrincipal.Current;
            ((IUnsafePerson)currentPrincipal).Person.Should().Be.EqualTo(newPerson);
            currentPrincipal.Identity.Should().Not.Be.EqualTo(threadPreviousIdentity);
        }

        [TearDown]
        public void Teardown()
        {
            Thread.CurrentPrincipal = new TeleoptiPrincipal(threadPreviousIdentity, threadPreviousPerson);
        }
    }
}
