using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;


namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class ShiftTradeAvailableCheckItemTest
	{
		[Test]
		public void ShouldGenerateCorrectHashValue()
		{
			var date = new DateOnly(2000, 1, 1);
			var personFrom = new Person();
			var personTo = new Person();

			var hashList = new HashSet<ShiftTradeAvailableCheckItem>();
			hashList.Add(new ShiftTradeAvailableCheckItem(date, personFrom, personTo)).Should().Be.True();
			hashList.Add(new ShiftTradeAvailableCheckItem(date, personFrom, personTo)).Should().Be.False();
		}
	}
}