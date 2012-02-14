using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    /// <summary>
    /// Tests for SessionData
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class SessionDataTest
    {
        private SessionData target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new SessionData();
        }

        /// <summary>
        /// Verifies the application data can be set.
        /// </summary>
        [Test]
        public void VerifyApplicationDataCanBeSet()
        {
            Assert.AreEqual(TimeZoneInfo.Utc.Id, target.TimeZone.Id);
        }

        /// <summary>
        /// Verifies the time zone can be set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-23
        /// </remarks>
        [Test]
        public void VerifyTimeZoneCanBeSet()
        {
            target.TimeZone = new CccTimeZoneInfo(TimeZoneInfo.Local);

            Assert.AreEqual(TimeZoneInfo.Local, target.TimeZone.TimeZoneInfoObject);
        }
    }
}