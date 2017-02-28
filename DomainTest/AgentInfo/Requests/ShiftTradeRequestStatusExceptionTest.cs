using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	[TestFixture]
	public class ShiftTradeRequestStatusExceptionTest : ExceptionTest<ShiftTradeRequestStatusException>
	{
		protected override ShiftTradeRequestStatusException CreateTestInstance(string message, Exception innerException)
		{
			return new ShiftTradeRequestStatusException(message, innerException);
		}

		protected override ShiftTradeRequestStatusException CreateTestInstance(string message)
		{
			return new ShiftTradeRequestStatusException(message);
		}
	}
}
