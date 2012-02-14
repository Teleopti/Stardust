using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Collection
{
    /// <summary>
    /// Tests for aggregateexception
    /// </summary>
    [TestFixture]
    public class DuplicateElementExceptionTest : ExceptionTest<DuplicateElementException>
    {
        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns></returns>
        protected override DuplicateElementException CreateTestInstance(string message, Exception innerException)
        {
            return new DuplicateElementException(message, innerException);
        }

        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected override DuplicateElementException CreateTestInstance(string message)
        {
            return new DuplicateElementException(message);
        }
    }
}