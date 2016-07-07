using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for aggregateexception
    /// </summary>
    [TestFixture]
	public class AggregateExceptionTest : ExceptionTest<Teleopti.Ccc.Domain.Common.AggregateException>
    {
        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns></returns>
		protected override Teleopti.Ccc.Domain.Common.AggregateException CreateTestInstance(string message, Exception innerException)
        {
			return new Teleopti.Ccc.Domain.Common.AggregateException(message, innerException);
        }

        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected override Teleopti.Ccc.Domain.Common.AggregateException CreateTestInstance(string message)
        {
			return new Teleopti.Ccc.Domain.Common.AggregateException(message);
        }
    }
}