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
			var messagesOnBoot = true;
			if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MessagesOnBoot"]))
				bool.TryParse(ConfigurationManager.AppSettings["MessagesOnBoot"], out messagesOnBoot);
			if (!messagesOnBoot)
				return null;
			if (!_toggleManager.IsEnabled(Toggles.LastHandlers_ToHangfire_41203))
				return null;

			return Task.Run(() => 
			{
				var start = parseNumber("InitialLoadScheduleProjectionStartDate", "-31");
				var end = parseNumber("InitialLoadScheduleProjectionEndDate", "180");
				using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
				{
					var tenants = _loadAllTenants.Tenants();
					_eventPublisher.Publish(new PublishInitializeReadModelEvent
					{
						InitialLoadScheduleProjectionStartDate = start,
						InitialLoadScheduleProjectionEndDate = end,
						Tenants = tenants.Select(t => t.Name).ToList()
					});
				}
			});
		}

		private static int parseNumber(string configString, string defaultValue)
		{
			var days = ConfigurationManager.AppSettings[configString];
			if (string.IsNullOrEmpty(days))
			{
				days = defaultValue;
			}
			int number;
			int.TryParse(days, out number);
			return number;
		}
	}
}