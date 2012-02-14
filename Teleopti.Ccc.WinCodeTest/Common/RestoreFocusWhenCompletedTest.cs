using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class RestoreFocusWhenCompletedTest
    {
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
        }

        [Test]
        public void VerifyThatRestoreFocusIsCalledWhenDisposed()
        {
            IFocusTarget target = _mocks.StrictMock<IFocusTarget>();
            RestoreFocusWhenCompleted restoreFocusWhenCompleted = new RestoreFocusWhenCompleted(target);

            using(_mocks.Record())
            {
                Expect.Call(target.SetFocus);

            }
            using(_mocks.Playback())
            {
                restoreFocusWhenCompleted.Dispose();
            }



        }
    }

    
   
}
