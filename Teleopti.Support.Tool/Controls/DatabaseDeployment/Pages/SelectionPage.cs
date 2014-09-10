using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
	public class SelectionPage : UserControl
	{
		public delegate void HasValidInputDelegate(bool isValid);
		public event HasValidInputDelegate hasValidInput;

		internal void triggerHasValidInput(bool isValid)
		{
			hasValidInput(isValid);
		}

		public virtual void GetData() { }
		public virtual void SetData() { }
	}
}
