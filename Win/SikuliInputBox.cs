using System.Windows.Forms;

namespace Teleopti.Ccc.Win
{
	public partial class SikuliInputBox : Form
	{

		public SikuliInputBox()
		{
			InitializeComponent();
		}

		public string GetValidatorName
		{
			get { return textBoxInput.Text; }
		}
	}
}
