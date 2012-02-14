using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	[Category("LongRunning")]
	public class ForeignKeyExceptionTest : ExceptionTest<ForeignKeyException>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), Test]
		public void VerifyConstructor()
		{
			const string mess = "sdfsdf";
			var inner = new Exception();
			const string sql = "sdf";
			var ex = new ForeignKeyException(mess, inner, sql);
			StringAssert.StartsWith(mess, ex.Message);
			Assert.AreSame(inner, ex.InnerException);
			Assert.AreEqual(sql, ex.Sql);
		}

		protected override ForeignKeyException CreateTestInstance(string message, Exception innerException)
		{
			return new ForeignKeyException(message, innerException);
		}

		protected override ForeignKeyException CreateTestInstance(string message)
		{
			return new ForeignKeyException(message);
		}
	}
}