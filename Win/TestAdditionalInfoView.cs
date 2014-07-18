using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win
{
	public partial class TestAdditionalInfoView : BaseRibbonForm
	{

		public TestAdditionalInfoView()
		{
			InitializeComponent();
			if (!DesignMode)
				SetTexts();
		}

		public TestAdditionalInfoView(string header)
			: this(header, string.Empty)
		{}


		public TestAdditionalInfoView(string header, string text) : this()
		{
			labelTestInfoHeader.Text = header;
			textBoxTestInfoText.Text = text;
		}

		public void CloseForm()
		{
			Close();
		}
	}
}
