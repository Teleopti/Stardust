using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.ApplicationConfig.Common;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.ApplicationConfigTest.Common
{
    [TestFixture]
    public class StateNewVersionTest
    {
        private StateNewVersion _target;
        private MockRepository mocks;
        private ISessionData sessionData;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            sessionData = mocks.StrictMock<ISessionData>();
            _target = new StateNewVersion();
        }

        [Test]
        public void VerifyCanGetProperties()
        {
            _target.SetSessionData(sessionData);
            Assert.AreEqual(sessionData, _target.SessionScopeData);
        }

        [Test]
        public void CanClearSession()
        {
            _target.SetSessionData(sessionData);
            _target.ClearSession();
            Assert.IsNull(_target.SessionScopeData);
        }
    }
}
