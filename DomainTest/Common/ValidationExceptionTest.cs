using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for validationexception
    /// </summary>
    [TestFixture]
    public class ValidationExceptionTest : ExceptionTest<ValidationException>
    {
        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns></returns>
        protected override ValidationException CreateTestInstance(string message, Exception innerException)
        {
            return new ValidationException(message, innerException);
        }

        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected override ValidationException CreateTestInstance(string message)
        {
            return new ValidationException(message);
        }
    }
}