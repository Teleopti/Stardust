using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	[Category("BucketB")]
	public class DataSourceExceptionTest : ExceptionTest<DataSourceException>
	{
		protected override DataSourceException CreateTestInstance(string message, Exception innerException)
		{
			return new DataSourceException(message, innerException);
		}

		protected override DataSourceException CreateTestInstance(string message)
		{
			return new DataSourceException(message);
		}
	}
}