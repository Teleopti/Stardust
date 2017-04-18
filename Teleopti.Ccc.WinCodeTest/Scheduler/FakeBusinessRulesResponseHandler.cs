using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	public class FakeBusinessRulesResponseHandler : IHandleBusinessRuleResponse
	{
		public void SetResponse(IEnumerable<IBusinessRuleResponse> businessRulesResponse)
		{
		}

		public FakeBusinessRulesResponseHandler WithDialogResult(DialogResult result)
		{
			DialogResult = result;
			return this;
		}

		public bool ApplyToAll { get; }
		public DialogResult DialogResult { get; private set; }
	}
}