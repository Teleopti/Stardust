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

		public TestAdditionalInfoView(string header) : this()
		{
			labelTestInfoHeader.Text = header;
		}

		public void CloseForm()
		{
			Close();
		}
	}
}
