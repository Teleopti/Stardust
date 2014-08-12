using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Shifts
{
	public partial class ShiftGenerationStatus : BaseDialogForm 
	{
		private readonly IWorkShiftAddCallbackWithEvent _callback;
		
		private IWin32Window _owner;
		
		public ShiftGenerationStatus()
		{
			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ShiftGenerationStatus(IWorkShiftAddCallbackWithEvent callback)
			: this()
		{
			_callback = callback;
			_callback.CountChanged +=callbackOnCountChanged;
			_callback.RuleSetReady += callbackOnRuleSetReady;
			_callback.RuleSetWarning += callbackOnRuleSetWarning;
			_callback.RuleSetToComplex += callbackRuleSetToComplex;	
		}

		void callbackRuleSetToComplex(object sender, EventArgs e)
		{
			Visible = false;
		}

		private void callbackOnRuleSetWarning(object sender, EventArgs eventArgs)
		{
			if (!IsDisposed && Visible == false)
				Show(_owner);
			
		}

		private void callbackOnRuleSetReady(object sender, EventArgs eventArgs)
		{
			Visible = false;
		}

		public void ShowDelayed(IWin32Window owner)
		{
			_owner = owner;
		}

		private void callbackOnCountChanged(object sender, EventArgs eventArgs)
		{
			if(IsDisposed) return;

			labelRuleSet.Text = _callback.CurrentRuleSetName;
			labelCount.Text = _callback.CurrentCount.ToString(CultureInfo.InvariantCulture);
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			_callback.Cancel();
		}

		private void shiftGenerationStatusLoad(object sender, EventArgs e)
		{
			buttonAdvCancel.Text = UserTexts.Resources.Cancel;
			labelCurrent.Text = UserTexts.Resources.CurrentNumberOfShifts;
			labelWarning.Text = UserTexts.Resources.ShiftGenerationWarning;
			Text = UserTexts.Resources.GeneratingShifts;
		}

	}
}
