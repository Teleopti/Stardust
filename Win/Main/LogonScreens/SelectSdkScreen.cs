using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectSdkScreen : UserControl, ILogonStep
	{
		private readonly ILogonView _logonView;
		private LogonModel _model;

		public SelectSdkScreen(ILogonView logonView, LogonModel model)
		{
		    _logonView = logonView;
		    _model = model;
            InitializeComponent();
	        labelChooseSDK.Text = Resources.PleaseChooseSDK;
			buttonLogOnCancel.Text = Resources.Cancel;
			buttonLogOnOK.Text = Resources.Ok;
			btnBack.Text = Resources.Back;
		}

        public void SetData()
		{
			lbxSelectSDK.DataSource = _model.Sdks;
		}

		public void GetData()
        {
            _model.SelectedSdk = lbxSelectSDK.SelectedItem.ToString();
        }

	    public void Release()
	    {
	       _model = null;
	        lbxSelectSDK.DataSource = null;
	    }

		public void SetBackButtonVisible(bool visible)
		{
			btnBack.Visible = visible;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			_logonView.HandleKeyPress(msg, keyData, lbxSelectSDK.Focused);
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void btnBack_Click(object sender, System.EventArgs e)
		{
			_logonView.BtnBackClick(sender, e);
		}

		private void buttonLogOnOK_Click(object sender, System.EventArgs e)
		{
			_logonView.ButtonLogOnOkClick(sender, e);
		}

		private void buttonLogOnCancel_Click(object sender, System.EventArgs e)
		{
			_logonView.ButtonLogOnCancelClick(sender, e);
		}

		private void lbxSelectSDK_DrawItem(object sender, DrawItemEventArgs e)
		{
			ListBox listBox = (ListBox)sender;
			e.DrawBackground();
			Brush myBrush = Brushes.Black;

			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				myBrush = Brushes.White;
				e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 153, 255)), e.Bounds);
			}

			else
			{
				e.Graphics.FillRectangle(Brushes.White, e.Bounds);

			}

			e.Graphics.DrawString(listBox.Items[e.Index].ToString() , e.Font, myBrush, e.Bounds);
			e.DrawFocusRectangle();
		}
	}
}
