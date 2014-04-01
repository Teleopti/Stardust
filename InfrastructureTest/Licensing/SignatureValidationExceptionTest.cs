#region Imports

using System;
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
    public class SignatureValidationExceptionTest : ExceptionTest<SignatureValidationException>
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
        protected override SignatureValidationException CreateTestInstance(string message, Exception innerException)
        {
            return new SignatureValidationException(message, innerException);
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
        protected override SignatureValidationException CreateTestInstance(string message)
        {
            return new SignatureValidationException(message);
        }
    }
}