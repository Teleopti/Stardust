using Autofac;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
	public class ApplicationStartup
	{
		private readonly IComponentContext _componentContext;
		private readonly ILogonPresenter _logonPresenter;
		private readonly WebUrlHolder _webUrlHolder;

		public ApplicationStartup(IComponentContext componentContext, ILogonPresenter logonPresenter, WebUrlHolder webUrlHolder)
		{
			_componentContext = componentContext;
			_logonPresenter = logonPresenter;
			_webUrlHolder = webUrlHolder;
		}

		/// <summary>
		/// Runs the shell application.
		/// </summary>
		public void LoadShellApplication()
		{
			var appl = new SmartClientShellApplication(_componentContext);
			appl.Run();
		}

		public bool LogOn()
		{
			return _logonPresenter.Start(_webUrlHolder.WebUrl);
		}
	}
}