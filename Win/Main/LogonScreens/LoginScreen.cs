using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class LoginScreen : UserControl, ILogonStep
	{
		private readonly LogonView _parent;
	    private readonly LogonModel _model;

	    public LoginScreen(LogonView parent, LogonModel model)
		{
			_parent = parent;
            _model = model;
            InitializeComponent();
            labelLogOn.Text = Resources.PleaseEnterYourLogonCredentials;
            labelLoginName.Text = Resources.LoginNameColon;
            labelPassword.Text = Resources.PasswordColon;
		}

        public void SetData(LogonModel model)
		{
        }

	    public LogonModel GetData()
	    {
            //Ska vi ha ett sätt att tala om för parent att det är ok att trycka Ok
	        _model.UserName = textBoxLogOnName.Text;
	        _model.Password = textBoxPassword.Text;
	        return _model;
	    }

	    private void LoginScreen_Load(object sender, System.EventArgs e)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(175, Color.CornflowerBlue);
        }
	}
}
