using System;
using System.Collections.Generic;
using Autofac;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Messaging.Client;

namespace Teleopti.Analytics.Etl.Common
{
	public class EtlModule : Module
	{
		private readonly IocConfiguration _configuration;

		public EtlModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EtlService>().SingleInstance();
			builder.RegisterType<EtlJobStarter>().SingleInstance();
			builder.RegisterType<PmInfoProvider>().SingleInstance();
			builder.RegisterType<JobExtractor>().SingleInstance();
			builder.RegisterType<JobHelper>().SingleInstance();
			
			if(_configuration.Toggle(Toggles.AddOrRemoveTenantsWithoutRestart_43635))
				builder.RegisterType<Tenants>().As<ITenants>().SingleInstance();
			else
				builder.RegisterType<TenantsOld>().As<ITenants>().SingleInstance();
			
			builder.RegisterType<TenantsLoadedInEtl>().As<IAllTenantNames>().SingleInstance();
			builder.RegisterType<BaseConfigurationRepository>().As<IBaseConfigurationRepository>().SingleInstance();
			builder.RegisterType<BadgeCalculationRepository>().As<IBadgeCalculationRepository>().SingleInstance();

			builder.RegisterType<FindTenantLogonInfoUnsecured>().As<IFindLogonInfo>().SingleInstance();
			builder.RegisterType<TenantLogonInfoLoader>().As<ITenantLogonInfoLoader>().SingleInstance();
			
			var url = new MutableUrl();
			url.Configure(_configuration.Args().MessageBrokerUrl);
			builder.RegisterInstance(url).As<IMessageBrokerUrl>();
		}

		public class TenantLogonInfoLoader : ITenantLogonInfoLoader
		{
			private readonly ITenantUnitOfWork _tenantUnitOfWork;
			private readonly IFindLogonInfo _findLogonInfo;

			public TenantLogonInfoLoader(ITenantUnitOfWork tenantUnitOfWork, IFindLogonInfo findLogonInfo)
			{
				_tenantUnitOfWork = tenantUnitOfWork;
				_findLogonInfo = findLogonInfo;
			}

			public IEnumerable<LogonInfo> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids)
			{
				using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
				{
					var ret = new List<LogonInfo>();
					foreach (var batch in personGuids.Batch(200))
					{
						ret.AddRange(_findLogonInfo.GetForIds(batch));
					}
					return ret;
				}
			}
		}
	}
}