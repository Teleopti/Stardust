using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(1000)]
	public class PublishStartupJobsTask : IBootstrapperTask
	{
		private readonly IToggleManager _toggleManager;
		private readonly IEventPublisher _eventPublisher;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;

		public PublishStartupJobsTask(IToggleManager toggleManager, IEventPublisher eventPublisher, ILoadAllTenants loadAllTenants, ITenantUnitOfWork tenantUnitOfWork)
		{
			_toggleManager = toggleManager;
			_eventPublisher = eventPublisher;
			_loadAllTenants = loadAllTenants;
			_tenantUnitOfWork = tenantUnitOfWork;
		}

		[TenantUnitOfWork]
		public virtual Task Execute(IAppBuilder application)
		{
			var messagesOnBoot = parseBool("MessagesOnBoot", true);
			if (!messagesOnBoot)
				return null;
			if (!_toggleManager.IsEnabled(Toggles.LastHandlers_ToHangfire_41203))
				return null;

			return Task.Run(() => 
			{
				var start = parseNumber("InitialLoadScheduleProjectionStartDate", -31);
				var end = parseNumber("InitialLoadScheduleProjectionEndDate", 180);
				using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
				{
					var tenants = _loadAllTenants.Tenants();
					Task.Delay(TimeSpan.FromSeconds(20)).ContinueWith(task => 
					{
						_eventPublisher.Publish(new PublishInitializeReadModelEvent
						{
							InitialLoadScheduleProjectionStartDate = start,
							InitialLoadScheduleProjectionEndDate = end,
							Tenants = tenants.Select(t => t.Name).ToList()
						});
					});
				}
			});
		}

		private static bool parseBool(string configString, bool defaultValue)
		{
			bool result;
			var value = ConfigurationManager.AppSettings[configString];
			if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out result))
				return result;
			return defaultValue;

		}

		private static int parseNumber(string configString, int defaultValue)
		{
			var days = ConfigurationManager.AppSettings[configString];
			if (string.IsNullOrEmpty(days))
			{
				return defaultValue;
			}
			int number;
			int.TryParse(days, out number);
			return number;
		}
	}
}