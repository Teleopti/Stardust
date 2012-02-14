using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("LongRunning")]
    public class ConstraintViolationExceptionTest : ExceptionTest<ConstraintViolationException>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), Test]
        public void VerifyConstructor()
        {
            const string mess = "sdfsdf"; 
            Exception inner = new Exception();
            string sql = "sdf";
            ConstraintViolationException ex = new ConstraintViolationException(mess, inner, sql);
            StringAssert.StartsWith(mess, ex.Message);
            Assert.AreSame(inner, ex.InnerException);
            Assert.AreEqual(sql, ex.Sql);
        }

        protected override ConstraintViolationException CreateTestInstance(string message, Exception innerException)
        {
            return new ConstraintViolationException(message, innerException);
        }

        protected override ConstraintViolationException CreateTestInstance(string message)
        {
            return new ConstraintViolationException(message);
        }
    }

}
