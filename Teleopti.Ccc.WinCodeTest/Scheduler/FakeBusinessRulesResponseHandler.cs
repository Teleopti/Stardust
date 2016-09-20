using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	public class FakeBusinessRulesResponseHandler : IHandleBusinessRuleResponse
	{
		public void SetResponse(IEnumerable<IBusinessRuleResponse> businessRulesResponse)
		{
		}

		public bool ApplyToAll { get; }
		public DialogResult DialogResult { get; }
	}
}