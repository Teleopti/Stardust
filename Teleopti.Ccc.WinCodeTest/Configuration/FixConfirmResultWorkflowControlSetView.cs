using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	public class FixConfirmResultWorkflowControlSetView : WorkflowControlSetView, IWorkflowControlSetView
	{
		private readonly bool _confirm;

		public FixConfirmResultWorkflowControlSetView(IToggleManager toggleManager, WorkflowControlSetPresenter presenter
			, bool confirm) : base(toggleManager, presenter)
		{
			_confirm = confirm;
		}

		public new bool ConfirmDeleteOfRequestPeriod()
		{
			return _confirm;
		}

	}
}