using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class LoginScreen : UserControl, ILogonStep
	{
		private readonly ILogonView _logonView;
		private LogonModel _model;

	    public LoginScreen(ILogonView logonView, LogonModel model)
		{
		    _logonView = logonView;
		    _model = model;
            InitializeComponent();
            
		}

        public void SetData()
		{
            ActiveControl = textBoxLogOnName;
        }

	    public void GetData()
	    {
	        _model.UserName = textBoxLogOnName.Text;
	        _model.Password = textBoxPassword.Text;
	    }

	    public void Release()
	    {
	        _model = null;
	    }

		public void SetBackButtonVisible(bool visible)
		{
			btnBack.Visible = visible;
		}

		private void LoginScreen_Enter(object sender, System.EventArgs e)
        {
            textBoxLogOnName.Select(0, textBoxLogOnName.Text.Length);
            textBoxLogOnName.Focus();
        }

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			_logonView.HandleKeyPress(msg, keyData, textBoxLogOnName.Focused || textBoxPassword.Focused);
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
	}
}
