using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Security
{
    /// <summary>
    /// Tests for InsufficientPrivilegeException
    /// </summary>
    [TestFixture]
    public class InsufficientPrivilegeExceptionTest : ExceptionTest<InsufficientPrivilegeException>
    {
        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns></returns>
        protected override InsufficientPrivilegeException CreateTestInstance(string message, Exception innerException)
        {
            return new InsufficientPrivilegeException(message, innerException);
        }

        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected override InsufficientPrivilegeException CreateTestInstance(string message)
        {
            return new InsufficientPrivilegeException(message);
        }
    }
}