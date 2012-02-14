using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.Domain.Time;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using System.Reflection;
using System.Globalization;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for the OutlierDateProviderBase abstract class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-14
    /// </remarks>
    [TestFixture]
    public class OutlierDateProviderBaseTest
    {
        private OutlierDateProviderBase target;
        private MockRepository mocks;
        
        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            target = mocks.StrictMock<OutlierDateProviderBase>();
        }

        /// <summary>
        /// Verifies the can create.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        [Test]
        public void VerifyCanCreate()
        {
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Verifies the name of the constructor with.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        [Test]
        public void VerifyConstructorWithName()
        {
            target = mocks.StrictMock<OutlierDateProviderBase>("theName");
            Expect.Call(target.Name).CallOriginalMethod(OriginalCallOptions.CreateExpectation).Repeat.Once();

            mocks.ReplayAll();
            
            Assert.AreEqual("theName", target.Name);

            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the name property.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        [Test]
        public void VerifyNameProperty()
        {
            target = mocks.StrictMock<OutlierDateProviderBase>();

            target.Name = "theName";
            LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation).Repeat.Once();

            Expect.Call(target.Name).CallOriginalMethod(OriginalCallOptions.CreateExpectation).Repeat.Once();

            mocks.ReplayAll();

            target.Name = "theName";
            Assert.AreEqual("theName", target.Name);

            mocks.VerifyAll();
        }
    }
}
