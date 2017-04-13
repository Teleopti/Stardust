using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Main
{
	public partial class LicenseAgreementForm : BaseDialogForm
	{
		public LicenseAgreementForm(string agreementText)
		{
			InitializeComponent();
			SetTexts();
			
			label1.Text = Resources.TeleoptiLicenseAgreementProgram;
			textBoxAgreement.Text = agreementText;
		}
	}
}
