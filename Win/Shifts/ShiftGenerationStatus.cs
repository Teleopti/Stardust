using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Shifts.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Shifts
{
	public partial class ShiftGenerationStatus : Form //BaseDialogForm
	{
		private readonly IWorkShiftAddCallback _callback;
		//private PerformanceCounter _ramCounter;
		
		private IWin32Window _owner;
		
		public ShiftGenerationStatus()
		{
			InitializeComponent();
		}

		public ShiftGenerationStatus(IWorkShiftAddCallback callback)
			: this()
		{
			_callback = callback;
			_callback.CountChanged +=callbackOnCountChanged;
			_callback.RuleSetReady += callbackOnRuleSetReady;
			_callback.RuleSetWarning += callbackOnRuleSetWarning;
			_callback.RuleSetToComplex += callbackRuleSetToComplex;
			//initializeRamCounter();
			//SetTexts();
		}

		void callbackRuleSetToComplex(object sender, EventArgs e)
		{
			Visible = false;
		}

		private void callbackOnRuleSetWarning(object sender, EventArgs eventArgs)
		{
			if (!IsDisposed && Visible == false)
			{
				Show(_owner);
			}
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

			label2.Text = _callback.CurrentCount.ToString(CultureInfo.InvariantCulture);

			//label3.Text = Convert.ToInt32(_ramCounter.NextValue()).ToString(CultureInfo.InvariantCulture) + "Mb";

			

		}

		private void buttonAdvCancel_Click(object sender, EventArgs e)
		{
			_callback.Cancel();
		}

		private void ShiftGenerationStatus_Load(object sender, EventArgs e)
		{

		}

		// private void initializeRamCounter()
		//{
		//	_ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
		//}

	}
}
