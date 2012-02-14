using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    /// <summary>
    /// Tests for ApplicationAuthenticationInfoTest
    /// </summary>
    [TestFixture]
    public class ApplicationAuthenticationInfoTest
    {
        private ApplicationAuthenticationInfo target;


        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new ApplicationAuthenticationInfo();
        }

        /// <summary>
        /// Verifies the default properties.
        /// </summary>
        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(string.Empty, target.ApplicationLogOnName);
            Assert.AreEqual(string.Empty, target.Password);
        }

        /// <summary>
        /// Verifies that the properties work.
        /// </summary>
        [Test]
        public void VerifyPropertiesWork()
        {
            string logOnName = "sdfsdfsdf";
            string password = "sdfjsdffff";
            target.ApplicationLogOnName = logOnName;
            target.Password=password;
            Assert.AreEqual(logOnName, target.ApplicationLogOnName);
            Assert.AreEqual(password, target.Password);
        }



        ///// <summary>
        ///// Verifies that logonname is not null nor empty.
        ///// </summary>
        //[Test]
        //public void VerifyLogOnNameIsNotNullNorEmpty()
        //{
        //    bool ok = false;
        //    try
        //    {
        //        target.ApplicationLogOnName = null;
        //    }
        //    catch (ArgumentException)
        //    {
        //        ok = true;
        //    }
        //    Assert.IsTrue(ok);
        //    ok = false;
        //    try
        //    {
        //        target.ApplicationLogOnName = string.Empty;
        //    }
        //    catch (ArgumentException)
        //    {
        //        ok = true;
        //    }
        //    Assert.IsTrue(ok);
        //}
    }
}