using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("LongRunning")]
    public class CannotConnectToDatabaseExceptionTest : ExceptionTest<CannotConnectToDatabaseException>
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [Test]
        public void VerifyConstructor()
        {
            const string mess = "sdfsdf"; 
            Exception inner = new Exception();
            string sql = "sdf";
            CannotConnectToDatabaseException ex = new CannotConnectToDatabaseException(mess, inner, sql);
            StringAssert.StartsWith(mess, ex.Message);
            Assert.AreSame(inner, ex.InnerException);
            Assert.AreEqual(sql, ex.Sql);
        }

        protected override CannotConnectToDatabaseException CreateTestInstance(string message, Exception innerException)
        {
            return new CannotConnectToDatabaseException(message, innerException);
        }

        protected override CannotConnectToDatabaseException CreateTestInstance(string message)
        {
            return new CannotConnectToDatabaseException(message);
        }
    }

}
