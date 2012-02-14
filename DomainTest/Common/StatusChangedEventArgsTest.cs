using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using System;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for the StatusChangedEventArgs class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-13
    /// </remarks>
    [TestFixture]
    public class StatusChangedEventArgsTest
    {
        private StatusChangedEventArgs target;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-13
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            target = new StatusChangedEventArgs("xxStatusText");
        }

        /// <summary>
        /// Verifies the properties.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-28
        /// </remarks>
        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("xxStatusText", target.StatusText);
        }

        /// <summary>
        /// Verifies the can set text.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-13
        /// </remarks>
        [Test]
        public void VerifyCanSetText()
        {
            string statusText = "xxMyNewStatus";
            target.StatusText = statusText;
            Assert.AreEqual(statusText, target.StatusText);
        }

        /// <summary>
        /// Checks the type of class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-13
        /// </remarks>
        [Test]
        public void CheckTypeOfClass()
        {
            Assert.IsInstanceOf<EventArgs>(target);
        }
    }
}
