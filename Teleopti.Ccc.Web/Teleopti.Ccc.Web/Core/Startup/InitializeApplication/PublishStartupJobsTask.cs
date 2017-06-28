using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Owin;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
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
		private readonly IDataSourceScope _dataSourceScope;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly Func<ICurrentUnitOfWork, IBusinessUnitRepository> _businessUnitRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private static readonly ILog logger = LogManager.GetLogger(typeof(PublishStartupJobsTask));

		public PublishStartupJobsTask(IToggleManager toggleManager, IEventPublisher eventPublisher, ILoadAllTenants loadAllTenants, IDataSourceScope dataSourceScope, ITenantUnitOfWork tenantUnitOfWork, Func<ICurrentUnitOfWork, IBusinessUnitRepository> businessUnitRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_toggleManager = toggleManager;
			_eventPublisher = eventPublisher;
			_loadAllTenants = loadAllTenants;
			_dataSourceScope = dataSourceScope;
			_tenantUnitOfWork = tenantUnitOfWork;
			_businessUnitRepository = businessUnitRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
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

			return Task.Delay(TimeSpan.FromSeconds(20)).ContinueWith(task => 
			{
				const int attempts = 3;
				for (var attempt = 1; attempt <= attempts; attempt++)
				{
					try
					{
						using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
						{
							var something = createInitialLoadScheduleProjectionEvents().ToArray();
							_eventPublisher.Publish(something);
						}
						break;
					}
					catch (Exception e)
					{
						logger.Warn($"An error occurred publishing Initial Load Schedule Projection Events, attempt {attempt} of {attempts}", e);
						Thread.Sleep(TimeSpan.FromSeconds(10));
					}
				}
			});
		}

		private IEnumerable<IEvent> createInitialLoadScheduleProjectionEvents()
		{
			var start = parseNumber("InitialLoadScheduleProjectionStartDate", "-31");
			var end = parseNumber("InitialLoadScheduleProjectionEndDate", "180");

			foreach (var tenant in _loadAllTenants.Tenants())
			{
				using (_dataSourceScope.OnThisThreadUse(tenant.Name))
				{
					using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						var businessUnits = _businessUnitRepository(new ThisUnitOfWork(uow)).LoadAll();
						foreach (var businessUnit in businessUnits)
						{
							yield return new InitialLoadScheduleProjectionEvent
							{
								LogOnDatasource = tenant.Name,
								LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(),
								StartDays = start,
								EndDays = end
							};
						}
					}
				}
			}
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