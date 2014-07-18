using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win
{
	public partial class TestAdditionalInfoView : Form
	{

		public TestAdditionalInfoView()
		{
			InitializeComponent();
		}

		public string Header
		{
			get { return labelTestInfoHeader.Text; }
			set { labelTestInfoHeader.Text = value; }
		}
	}
}
