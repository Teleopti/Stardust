#region Imports

using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.TestCommon;

#endregion

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [TestFixture]
	[Category("LongRunning")]
	[CLSCompliant(false)]
    public class TooManyActiveAgentsExceptionTest : ExceptionTest<TooManyActiveAgentsException>
    {
        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-11-18
		/// </remarks>
		[CLSCompliant(false)]
        protected override TooManyActiveAgentsException CreateTestInstance(string message, Exception innerException)
        {
            return new TooManyActiveAgentsException(message, innerException);
        }

        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-11-18
		/// </remarks>
		[CLSCompliant(false)]
        protected override TooManyActiveAgentsException CreateTestInstance(string message)
        {
            return new TooManyActiveAgentsException(message);
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void TestSpecialConstructor()
        {
            const int numberOfLicensedActiveAgents = 100;
            const int numberOfAttemptedActiveAgents = 120;
            TooManyActiveAgentsException e = new TooManyActiveAgentsException(numberOfLicensedActiveAgents,
                                                                              numberOfAttemptedActiveAgents);
            Assert.AreEqual(numberOfLicensedActiveAgents, e.NumberOfLicensed);
            Assert.AreEqual(numberOfAttemptedActiveAgents, e.NumberOfAttemptedActiveAgents);
        }
    }
}