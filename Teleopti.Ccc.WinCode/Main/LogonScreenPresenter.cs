using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonScreenPresenter
	{
		void OkbuttonClicked(LoginStep currentStep);
		void BackButtonClicked(LoginStep currentStep);
		void InitializeLogin();
	}

	public class LogonScreenPresenter : ILogonScreenPresenter, ILicenseFeedback
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

		private bool setupCulture()
		{
			if (TeleoptiPrincipal.Current.Regional == null) return false;

			Thread.CurrentThread.CurrentCulture =
				TeleoptiPrincipal.Current.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture =
				TeleoptiPrincipal.Current.Regional.UICulture;
			return true;
		}

		private bool initializeLicense()
		{
			var unitofWorkFactory = _dataSource.DataSource.Application;
			var verifier = new LicenseVerifier(this, unitofWorkFactory, new LicenseRepository(unitofWorkFactory));
			var licenseService = verifier.LoadAndVerifyLicense();
			if (licenseService == null)
				return false;
			return true;
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
