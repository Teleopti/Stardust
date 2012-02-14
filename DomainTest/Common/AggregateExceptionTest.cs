using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for aggregateexception
    /// </summary>
    [TestFixture]
    public class AggregateExceptionTest : ExceptionTest<AggregateException>
    {
        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns></returns>
        protected override AggregateException CreateTestInstance(string message, Exception innerException)
        {
            return new AggregateException(message, innerException);
        }

        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected override AggregateException CreateTestInstance(string message)
        {
            return new AggregateException(message);
        }
    }
}