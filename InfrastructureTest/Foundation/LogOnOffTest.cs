using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    /// <summary>
    /// Tests for LogOnOff
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class LogOnOffTest
    {
        private MockRepository mocks;
        private ILogOnOff target;
         
        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            target = new LogOnOff(new PrincipalManager(new TeleoptiPrincipalFactory()));
            StateHolderProxyHelper.ClearAndSetStateHolder(mocks.StrictMock<IState>());
        }

        /// <summary>
        /// Verifies the log off calls clearsession.
        /// </summary>
        [Test]
        public void VerifyLogOffCallsClearSession()
        {
            IState stateMock = mocks.StrictMock<IState>();
            using (mocks.Record())
            {
                StateHolderProxyHelper.ClearAndInitializeStateHolder(stateMock);
                stateMock.ClearSession();
            }
            using (mocks.Playback())
            {
                target.LogOff();
            }
        }
    }
}