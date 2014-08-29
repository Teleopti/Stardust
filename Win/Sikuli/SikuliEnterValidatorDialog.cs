using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Sikuli
{
	public partial class SikuliEnterValidatorDialog : Form
	{

		public SikuliEnterValidatorDialog()
		{
			InitializeComponent();
		}

		public string GetValidatorName
		{
			get { return textBoxInput.Text; }
		}
	}
}
