using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeShiftTradeLightValidator : IShiftTradeLightValidator
	{
		public ShiftTradeRequestValidationResult Validate(ShiftTradeAvailableCheckItem checkItem)
		{
			
			return
				new ShiftTradeRequestValidationResult(ValidateByName(checkItem.PersonFrom.Name) &&
				                                      ValidateByName(checkItem.PersonTo.Name));
			
		}


		private bool ValidateByName(Name name)
		{
			var nameConcated = name.LastName + " " + name.FirstName;
			return !nameConcated.Contains("NoShiftTrade");	
		}
	}
}
