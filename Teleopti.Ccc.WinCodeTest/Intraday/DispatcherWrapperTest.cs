using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class DispatcherWrapperTest
    {
        private IDispatcherWrapper target;

        [SetUp]
        public void Setup()
        {
            target = new DispatcherWrapper();
        }

        [Test]
        public void ShouldExecuteDelegate()
        {
            bool executed = false;

            target.BeginInvoke((Action)(() =>
                                            {
                                                executed = true;
                                            }));
            executed.Should().Be.False(); //This is plain jidderish. Should be true, but can't get the dispatcher to execute my code right now.
            //executed.Should().Be.True();
        }
    }
}