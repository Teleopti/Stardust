using System.ComponentModel;
using Teleopti.Ccc.WinCode.Budgeting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting.Events
{
	public class CancelEventModel : ICancelEventModel
	{
		private readonly CancelEventArgs _cancelEventArgs;

		public CancelEventModel(CancelEventArgs cancelEventArgs)
		{
			_cancelEventArgs = cancelEventArgs;
		}

		public bool CancelEvent
		{
			get { return _cancelEventArgs.Cancel; }
			set { _cancelEventArgs.Cancel = value; }
		}
	}
}