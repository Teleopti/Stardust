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
            labelLogOn.Text = Resources.PleaseEnterYourLogonCredentials;
            labelLoginName.Text = Resources.LoginNameColon;
            labelPassword.Text = Resources.PasswordColon;
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
	}
}
