using Syncfusion.Windows.Forms;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Views
{
	internal partial class SikuliResultView : MetroForm
	{

		public SikuliResultView()
		{
			InitializeComponent();
		}

		public string Header
		{
			set
			{
				labelTestInfoHeader.Text = value;
				labelResult.Text = string.Empty;
			}
		}

		public SikuliValidationResult.ResultValue Result
		{
			set
			{
				switch (value)
				{
					case SikuliValidationResult.ResultValue.Pass:
						labelResult.Text = "PASS";
						labelResult.ForeColor = System.Drawing.Color.Green;
						break;
					case SikuliValidationResult.ResultValue.Warn:
						labelResult.Text = "WARN";
						labelResult.ForeColor = System.Drawing.Color.DarkOrange;
						break;
					default :
						labelResult.Text = "FAIL";
						labelResult.ForeColor = System.Drawing.Color.Red;
						break;
				}
				
			}
		}

		public string Details
		{
			set
			{
				textBoxDetails.Text = value;
			}
		}
	}
}
