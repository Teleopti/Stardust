using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Security
{
    /// <summary>
    /// Tests for PermissionException
    /// </summary>
    [TestFixture]
    public class PermissionExceptionTest : ExceptionTest<PermissionException>
    {
        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns></returns>
        protected override PermissionException CreateTestInstance(string message, Exception innerException)
        {
            return new PermissionException(message, innerException);
        }

        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected override PermissionException CreateTestInstance(string message)
        {
            return new PermissionException(message);
        }
    }
}