using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class CouldNotCreateConnectionExceptionTest : ExceptionTest<CouldNotCreateConnectionException>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), Test]
		public void VerifyConstructor()
		{
			const string mess = "sdfsdf";
			var inner = new Exception();
			const string sql = "sdf";
			var ex = new CouldNotCreateConnectionException(mess, inner, sql);
			StringAssert.StartsWith(mess, ex.Message);
			Assert.AreSame(inner, ex.InnerException);
			Assert.AreEqual(sql, ex.Sql);
		}

		protected override CouldNotCreateConnectionException CreateTestInstance(string message, Exception innerException)
		{
			return new CouldNotCreateConnectionException(message, innerException);
		}

		protected override CouldNotCreateConnectionException CreateTestInstance(string message)
		{
			return new CouldNotCreateConnectionException(message);
		}
	}
}