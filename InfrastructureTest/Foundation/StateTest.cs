using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    /// <summary>
    /// Tests for State
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class StateTest
    {
        private State target;
        private MockRepository mocks;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            target = mocks.PartialMock<State>();
        }

        /// <summary>
        /// Verifies the can create and properties are right.
        /// </summary>
        [Test]
        public void VerifyCanCreateAndPropertiesAreRight()
        {
            mocks.ReplayAll();
            Assert.IsNotNull(target);
            Assert.IsNull(target.ApplicationScopeData);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the set application data.
        /// </summary>
        [Test]
        public void VerifySetApplicationData()
        {
            IApplicationData appData = mocks.StrictMock<IApplicationData>();
            mocks.ReplayAll();
            target.SetApplicationData(appData);
            Assert.AreSame(appData, target.ApplicationScopeData);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the logged in true.
        /// </summary>
        [Test]
        public void VerifyLoggedInTrue()
        {
            using(mocks.Record())
            {
                Expect.Call(target.IsLoggedIn).CallOriginalMethod(OriginalCallOptions.CreateExpectation);
            }
            using(mocks.Playback())
            {
                Assert.IsTrue(target.IsLoggedIn);
            }
        }

        /// <summary>
        /// Verifies the logged in false.
        /// </summary>
        [Test]
        public void VerifyLoggedInFalse()
        {
            var currentPrincipal = TeleoptiPrincipal.Current;
            var identity = (TeleoptiIdentity)currentPrincipal.Identity;

            currentPrincipal.ChangePrincipal(
                new TeleoptiPrincipal(new TeleoptiIdentity("", identity.DataSource, identity.BusinessUnit,
                                                           identity.WindowsIdentity),((IUnsafePerson)currentPrincipal).Person));
            Assert.IsFalse(target.IsLoggedIn);
            
            currentPrincipal.ChangePrincipal(new TeleoptiPrincipal(identity,((IUnsafePerson)currentPrincipal).Person));
        }
    }
}