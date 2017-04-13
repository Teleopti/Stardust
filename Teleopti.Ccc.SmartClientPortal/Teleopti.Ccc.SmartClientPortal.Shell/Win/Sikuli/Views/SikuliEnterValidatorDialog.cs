using Syncfusion.Windows.Forms;

namespace Teleopti.Ccc.Win.Sikuli.Views
{
	internal partial class SikuliEnterValidatorDialog : MetroForm
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
