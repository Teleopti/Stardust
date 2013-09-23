using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonPresenter
	{
		void OkbuttonClicked(LoginStep currentStep);
		void BackButtonClicked(LoginStep currentStep);
		bool InitializeLogin();
	}

	public class LogonPresenter : ILogonPresenter
	{
		private readonly ILogonView _view;
		private readonly ILogonModel _model;
		private readonly ILoginInitializer _initializer;

		private DataSourceContainer _dataSource;
		
		public LogonPresenter(ILogonView view, ILogonModel model, ILoginInitializer initializer)
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
