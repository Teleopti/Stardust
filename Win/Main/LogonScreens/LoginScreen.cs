using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class LoginScreen : UserControl, ILogonStep
	{
		private LogonModel _model;

	    public LoginScreen(LogonModel model)
		{
			_model = model;
            InitializeComponent();
            labelLogOn.Text = Resources.PleaseEnterYourLogonCredentials;
            labelLoginName.Text = Resources.LoginNameColon;
            labelPassword.Text = Resources.PasswordColon;
		}

        public void SetData(LogonModel model)
		{
            ActiveControl = textBoxLogOnName;
        }

	    public LogonModel GetData()
	    {
            //Ska vi ha ett sätt att tala om för parent att det är ok att trycka Ok
	        _model.UserName = textBoxLogOnName.Text;
	        _model.Password = textBoxPassword.Text;
	        return _model;
	    }

	    public void Release()
	    {
	        _model = null;
	    }

	    private void LoginScreen_Load(object sender, System.EventArgs e)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(175, Color.CornflowerBlue);
        }

        private void LoginScreen_Enter(object sender, System.EventArgs e)
        {
            textBoxLogOnName.Select(0, textBoxLogOnName.Text.Length);
            textBoxLogOnName.Focus();
        }
	}
}
