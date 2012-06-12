using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WinCode.FileImport;

namespace Teleopti.Ccc.WinCodeTest.FileImport
{
	[TestFixture]
	public class FileImportExceptionTest : ExceptionTest<FileImportException>
	{
		protected override FileImportException CreateTestInstance(string message, Exception innerException)
		{
			return new FileImportException(message, innerException);
		}

		protected override FileImportException CreateTestInstance(string message)
		{
			return new FileImportException(message);
		}
	}
}
