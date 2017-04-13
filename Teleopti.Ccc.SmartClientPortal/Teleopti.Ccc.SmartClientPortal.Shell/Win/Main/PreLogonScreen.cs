using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Main
{
	public partial class PreLogonScreen : MetroForm
	{
		public PreLogonScreen()
		{
			InitializeComponent();
		}

		public PreLogonScreen(IEnumerable<string> itemList)
		{
			InitializeComponent();
			if (!DesignMode)
				runTimeDesign();
			comboBoxAdvSDKList.DataSource = itemList.ToList();
			comboBoxAdvSDKList.Select();
		}

		public string GetData()
		{
			return comboBoxAdvSDKList.SelectedItem.ToString();
		}

		private void runTimeDesign()
		{
			comboBoxAdvSDKList.Style = VisualStyle.Metro;
		}
	}
}
