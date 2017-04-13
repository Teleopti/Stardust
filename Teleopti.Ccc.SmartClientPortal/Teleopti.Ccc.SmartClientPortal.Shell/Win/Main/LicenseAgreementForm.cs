using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Main
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
