using Autofac;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
    public class ApplicationStartup
    {
        private readonly IComponentContext _componentContext;
        private readonly ILogonPresenter _logonPresenter;

		public ApplicationStartup(IComponentContext componentContext, ILogonPresenter logonPresenter)
        {
            _componentContext = componentContext;
		    _logonPresenter = logonPresenter;
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
        public bool LogOn()
        {
            return _logonPresenter.Start();
        }
    }
}