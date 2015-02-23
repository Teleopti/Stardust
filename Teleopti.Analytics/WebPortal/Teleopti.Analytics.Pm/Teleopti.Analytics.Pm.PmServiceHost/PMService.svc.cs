using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.ServiceModel;
using log4net;
using log4net.Config;
using Teleopti.Analytics.Portal.AnalyzerProxy;

namespace Teleopti.Analytics.PM.PMServiceHost
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class PMService : IPMService
	{
		private class DataSourceCheck
		{
			public bool IsCheckDone { get; set; }
			public bool Result { get; set; }
		}

		private DataSourceCheck _dataSourceCheck;
		private readonly ILog _logger;
		private List<UserDto> _clientUsersToSynchronize;

		public PMService()
		{
			XmlConfigurator.Configure();
			_logger = LogManager.GetLogger(typeof(PMService));
			_clientUsersToSynchronize = new List<UserDto>();
		}

		[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public ResultDto AddUsersToSynchronize(UserDto[] users)
		{
			var resultDto = new ResultDto { Success = false };

			if (!PermissionInformation.IsPmAuthenticationWindows)
			{
				resultDto.Success = true;
				return resultDto;
			}

			if (_clientUsersToSynchronize == null)
			{
				var pme = new PmSynchronizeException("Cannot add users to be synchronized since field variable _clientUsersToSynchronize is not instantiated!");
				resultDto.ErrorMessage = pme.Message;
				resultDto.ErrorType = pme.GetType().ToString();
				return resultDto;
			}

			if (users != null && users.Length > 0)
			{
				_logger.DebugFormat("Before adding users to synchronize. Users to sync: {0}. Users to add: {1}", _clientUsersToSynchronize.Count, users.Length);
				_clientUsersToSynchronize.AddRange(users);
				resultDto.Success = true;
				_logger.DebugFormat("After adding users to synchronize. Users to sync: {0}. ", _clientUsersToSynchronize.Count);
			}
			else
			{
				_logger.Debug("User array is either null contains no elements in call to AddUsersToSynchronize(UserDto[] users).");
			}

			return resultDto;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public ResultDto SynchronizeUsers(string olapServer, string olapDatabase)
		{
			var resultDto = new ResultDto();

			try
			{
				using (var analyzerProxy = new ClientProxy(olapServer, olapDatabase))
				{
					if (ensureDataSourceExists(analyzerProxy, olapServer, out resultDto))
					{
						var synchronizer = new Synchronizer(_clientUsersToSynchronize, new UserHandler(analyzerProxy));
						resultDto = synchronizer.SynchronizeUsers();
					}

					analyzerProxy.LogOffUser();
				}
			}
			catch (PmSynchronizeException pme)
			{
				_logger.ErrorFormat("Exception in PMService.SynchronizeUsers() of type PmSynchronizeException thrown. Message: {0}", pme.Message);
				resultDto.ErrorMessage = pme.Message;
				resultDto.ErrorType = pme.GetType().ToString();
			}
			catch (Exception e)
			{
				_logger.ErrorFormat("Exception in PMService.SynchronizeUsers() thrown. Message: {0}", e.Message);
				resultDto.ErrorMessage = e.Message;
				resultDto.ErrorType = e.GetType().ToString();
			}

			return resultDto;
		}

		[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public void ResetUserLists()
		{
			_clientUsersToSynchronize = new List<UserDto>();
		}

		[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public ResultDto IsWindowsAuthentication(string olapServer, string olapDatabase)
		{
			var resultDto = new ResultDto();
			try
			{
				using (var analyzerProxy = new ClientProxy(olapServer, olapDatabase))
				{
					if (ensureDataSourceExists(analyzerProxy, olapServer, out resultDto))
					{
						resultDto.Success = true;
						resultDto.IsWindowsAuthentication = PermissionInformation.IsPmAuthenticationWindows;
					}

					analyzerProxy.LogOffUser();
				}
			}
			catch (PmSynchronizeException pme)
			{
				_logger.ErrorFormat("Exception in PMService.IsWindowsAuthentication() of type PmSynchronizeException thrown. Message: {0}", pme.Message);
				resultDto.ErrorMessage = pme.Message;
				resultDto.ErrorType = pme.GetType().ToString();
			}
			catch (Exception e)
			{
				_logger.ErrorFormat("Exception in PMService.IsWindowsAuthentication() thrown. Message: {0}", e.Message);
				resultDto.ErrorMessage = e.Message;
				resultDto.ErrorType = e.GetType().ToString();
			}

			return resultDto;
		}

		private bool ensureDataSourceExists(ClientProxy analyzerProxy, string olapServer, out ResultDto resultDto)
		{
			resultDto = new ResultDto { Success = true };
			if (_dataSourceCheck != null)
				return _dataSourceCheck.Result;

			_dataSourceCheck = new DataSourceCheck { IsCheckDone = true };

			checkIdentity();

			if (analyzerProxy.EnsureDataSourceExists())
			{
				_dataSourceCheck.Result = true;
				return true;
			}

			var pme = new PmSynchronizeException(string.Format("Could not create Analyzer data source '{0}'.", olapServer));
			resultDto.Success = false;
			resultDto.ErrorMessage = pme.Message;
			resultDto.ErrorType = pme.GetType().ToString();
			return false;
		}

		private void checkIdentity()
		{
			WindowsIdentity callerWindowsIdentity = ServiceSecurityContext.Current.WindowsIdentity;
			if (callerWindowsIdentity == null)
			{
				throw new InvalidOperationException("The caller cannot be mapped to a Windows Identity");
			}
			_logger.DebugFormat("Before contacting Analyzer the client caller is: '{0}'", callerWindowsIdentity.Name);
		}
	}
}
