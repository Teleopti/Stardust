using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Core.Internal;
using log4net;
using Owin;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(101)] //priority after TenantStartupTask
	[EnabledBy(Toggles.Wfm_AutomaticNotificationEnrollment_79679)]
	public class NotificationSubscriptionTask : IBootstrapperTask
	{
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly INotificationServiceClient _notificationServiceClient;
		private readonly IApplicationConfigurationDbProvider _applicationConfigurationDbProvider;
		private readonly IInstallationEnvironment _installationEnvironment;
		private readonly ILog _logger = LogManager.GetLogger(typeof(NotificationSubscriptionTask));
		private readonly IToggleManager _toggleManager;

		public NotificationSubscriptionTask(
			ILoadAllTenants tenants, 
			ITenantUnitOfWork tenantUnitOfWork, 
			INotificationServiceClient notificationServiceClient,
			IApplicationConfigurationDbProvider applicationConfigurationDbProvider,
			IInstallationEnvironment installationEnvironment,
			IToggleManager toggleManager)
		{
			_loadAllTenants = tenants;
			_tenantUnitOfWork = tenantUnitOfWork;
			_notificationServiceClient = notificationServiceClient;
			_applicationConfigurationDbProvider = applicationConfigurationDbProvider;
			_installationEnvironment = installationEnvironment;
			_toggleManager = toggleManager;
		}

		public NotificationResult LastExecution { get; set; }

		public Task Execute(IAppBuilder application)
		{
			return Task.Run(() =>
			{
				LastExecution = null;
				var returnResult = new NotificationResult();
				if (_installationEnvironment.IsAzure && _installationEnvironment.RoleInstanceID > 0)
				{
					returnResult.IsSuccess = false;
					returnResult.Message = NotificationMessage.NotOnFirstRoleInstanceID;
					LastExecution = returnResult;
				}

				using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
				{
					var subscriptionApiEndpoint = _applicationConfigurationDbProvider.GetServerValue(
						ServerConfigurationKey.NotificationSubscriptionApiEndpoint);

					var subscriptionApiEndpointKey = _applicationConfigurationDbProvider.GetServerValue(
						ServerConfigurationKey.NotificationSubscriptionApiEndpointKey);

					if (subscriptionApiEndpoint.IsNullOrEmpty() || subscriptionApiEndpointKey.IsNullOrEmpty())
					{
						returnResult.Message = NotificationMessage.ServerConfigurationKeysNotPresent;
						returnResult.IsSuccess = false;
						LastExecution = returnResult;
						return;
					}

					foreach (var tenant in _loadAllTenants.Tenants())
					{
						tenant.ApplicationConfig.TryGetValue(TenantApplicationConfigKey.NotificationApiKey.ToString(), 
							out var apiKey);

						if (!apiKey.IsNullOrEmpty()) continue;

						var subscriptionKey = string.Empty;

						try
						{
							
							//never want to call Azure if a local environment							
							if (Environment.UserDomainName == "TOPTINET" && 
								!_toggleManager.IsEnabled(Toggles.Wfm_AutomaticNotificationEnrollmentStartup_79679))
							{								
								subscriptionKey = "5bb3723b10d44163800b83aab080d603";
								returnResult.Keys.Add(subscriptionKey);
							}
							else
							{
								var result = _notificationServiceClient.CreateSubscription(tenant.Name,
									subscriptionApiEndpoint,
									subscriptionApiEndpointKey,
									_installationEnvironment.IsAzure);

								returnResult.Keys.Add(result.subscriptionKey);
								subscriptionKey = result.subscriptionKey;
							}
						}
						catch (Exception ex)
						{
							_logger.Error("notificationServiceClient error", ex);
						}

						if (!string.IsNullOrEmpty(subscriptionKey))
						{
							tenant.SetApplicationConfig(TenantApplicationConfigKey.NotificationApiKey, subscriptionKey);
						}
						else
						{
							_logger.Info($"subscriptionkey was empty for tenant {tenant.Name}");
						}
					}

					_tenantUnitOfWork.CommitAndDisposeCurrent();
				}

				returnResult.IsSuccess = true;
				LastExecution = returnResult;
			});
			
		}
	}

	public class NotificationResult
	{
		public NotificationResult()
		{
			Keys = new List<string>();
			IsSuccess = false;
		}

		public NotificationMessage Message { get; set; }

		public bool IsSuccess { get; set; }

		public List<string> Keys { get; set; }

	}

	public enum NotificationMessage
	{
		ServerConfigurationKeysNotPresent,
		NotOnFirstRoleInstanceID
	}
}