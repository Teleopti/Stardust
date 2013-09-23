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
		void InitializeLogin();
	}

	public class LogonScreenPresenter : ILogonScreenPresenter
	{
		private readonly ILogonScreenView _view;
		private readonly ILogonScreenModel _model;

		private DataSourceContainer _dataSource;

		private delegate bool LoadingFunction();
		private readonly IEnumerable<LoadingFunction> _initializeFunctions;

		public LogonScreenPresenter(ILogonScreenView view, ILogonScreenModel model)
		{
			_view = view;
			_model = model;
			
			_initializeFunctions = new List<LoadingFunction>
				{
					setupCulture,
					initializeLicense,
					checkRaptorApplicationFunctions
				};
		}

		private bool checkRaptorApplicationFunctions()
		{
			throw new NotImplementedException();
		}

		public void OkbuttonClicked(LoginStep currentStep)
		{
			throw new NotImplementedException();
		}

		public void BackButtonClicked(LoginStep currentStep)
		{
			throw new NotImplementedException();
		}

		public void InitializeLogin()
		{
			if (_initializeFunctions.All(m => m()))
				;
			else
				_view.Exit();

		}


		public void Warning(string warning)
		{
			throw new NotImplementedException();
		}

		public void Error(string error)
		{
			throw new NotImplementedException();
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
