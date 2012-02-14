using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class CustomEventArgsTest
    {
        private CustomEventArgs<string> target;

        [Test]
        public void CanCreateInstance()
        {
            target= new CustomEventArgs<string>("test");
            Assert.AreEqual("test",target.Value);
        }

        [Test]
        public void VerifyTargetExtendsEventArgs()
        {
            Assert.IsInstanceOf<EventArgs>(target);
        }
    }
}