using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonScreenPresenter
	{
		void OkbuttonClicked(LoginStep currentStep);
		void BackButtonClicked(LoginStep currentStep);
		bool InitializeLogin();
	}

	public class LogonScreenPresenter : ILogonScreenPresenter
	{
		private readonly ILogonScreenView _view;
		private readonly ILogonScreenModel _model;
		private readonly ILoginInitializer _initializer;

		private DataSourceContainer _dataSource;
		
		public LogonScreenPresenter(ILogonScreenView view, ILogonScreenModel model, ILoginInitializer initializer)
		{
			_view = view;
			_model = model;
			_initializer = initializer;
		}

		public void OkbuttonClicked(LoginStep currentStep)
		{
			throw new NotImplementedException();
		}

		public void BackButtonClicked(LoginStep currentStep)
		{
			throw new NotImplementedException();
		}

		public bool InitializeLogin()
		{
			return _initializer.InitializeApplication();
		}
	}

	public enum LoginStep
	{
		Initializing = 0,
		SelectSdk = 1,
		SelectDatasource = 2,
		Login = 3,
		Loading = 4,
		Ready = 5
	}
}
