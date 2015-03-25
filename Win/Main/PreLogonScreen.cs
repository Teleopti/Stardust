using System.Collections.Generic;
using Syncfusion.Windows.Forms;

namespace Teleopti.Ccc.Win.Main
{
	public partial class PreLogonScreen : MetroForm
	{
		private readonly IList<string> _itemList;

		public PreLogonScreen()
		{
			InitializeComponent();
		}

		public PreLogonScreen(IList<string> itemList)
		{
			_itemList = itemList;
			InitializeComponent();
			if (!DesignMode)
				runTimeDesign();
			comboBoxAdvSDKList.DataSource = _itemList;
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
