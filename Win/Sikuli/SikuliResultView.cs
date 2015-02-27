using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Sikuli
{
	internal partial class SikuliResultView : Form
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
