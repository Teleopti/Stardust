using System.Drawing;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
    public class ApplicationStartup
    {
        private readonly IComponentContext _componentContext;
        private readonly ILogonPresenter _logonPresenter;
        //private readonly LogonView _logOnScreen;

		public ApplicationStartup(IComponentContext componentContext, ILogonPresenter logonPresenter)
        {
            _componentContext = componentContext;
		    _logonPresenter = logonPresenter;
		    //_logOnScreen = (LogonView)logOnscreen;
        }

        /// <summary>
        /// Runs the shell application.
        /// </summary>
        public void LoadShellApplication()
        {
            var appl = new SmartClientShellApplication(_componentContext);
            appl.Run();
        }

        /// <summary>
        /// Loads the authentication form.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        public bool LogOn(Point startPosition)
        {
            return _logonPresenter.Start(startPosition);
            //_logOnScreen.ShowDialog();
            //if (_logOnScreen.IsDisposed)
            //    return false;

            //_logOnScreen.Show();
            //_logOnScreen.Refresh();

            //return (_logOnScreen.DialogResult == DialogResult.OK);
            //return true;
        }
    }
}