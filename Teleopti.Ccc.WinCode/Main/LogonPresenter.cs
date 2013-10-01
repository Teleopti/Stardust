using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonPresenter
	{
		void OkbuttonClicked(object data);
		void BackButtonClicked();
		bool InitializeLogin(string endpoint);
		bool InitializeLogin(string nhibConfigPath, string isBrokerDisabled);
		List<string> GetDataForCurrentStep();
	}

	public class LogonPresenter //: ILogonPresenter
	{
		private readonly ILogonView _view;
		private readonly LogonModel _model;
		private readonly ILoginInitializer _initializer;
		private readonly IDataSourceHandler _dataSource;

		public LogonPresenter(ILogonView view, LogonModel model,
		                      IDataSourceHandler dataSource,
		                      ILoginInitializer initializer)
		{
			_view = view;
			_model = model;
			_dataSource = dataSource;
			_initializer = initializer;
			_view.StartLogon();
		}

		public void OkbuttonClicked(object data)
		{
			switch (_view.CurrentStep)
			{
				case LoginStep.SelectSdk:
					_model.ServerName = data.ToString();
					_view.StepForward();
					break;
				case LoginStep.SelectDatasource:
					_model.DataSource = data.ToString();
					break;
				case LoginStep.Login:
					break;
				case LoginStep.Ready:
					break;
				case LoginStep.Loading:
					_initializer.InitializeApplication();
					break;
			}
		}

		public void BackButtonClicked()
		{
			throw new NotImplementedException();
		}

		public bool InitializeLogin(string getEndpointNames)
		{
			return true;
		}

		public bool InitializeLogin(string getEndpointNames, string isBrokerDisabled)
		{
			return true;
		}

		public object GetDataForCurrentStep()
		{
			switch (_view.CurrentStep)
			{
				case LoginStep.SelectDatasource:
					return new List<string>{ "Hejsan hoppsan"};
				case LoginStep.Login:
					break;
				default:
					return null;
			}
			return null;
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
