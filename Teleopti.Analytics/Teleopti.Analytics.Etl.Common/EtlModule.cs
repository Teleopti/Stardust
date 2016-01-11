using System;
using System.Collections.Generic;
using Autofac;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Analytics.Etl.Common
{
	public class EtlModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public EtlModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EtlService>().SingleInstance();


			//TODO: tenant - use tenantmodule instead (with some override for special cases?)?
			builder.Register(c =>
			{
				var configReader = c.Resolve<IConfigReader>();
				var connstringAsString = configReader.ConnectionString("Tenancy");
				return TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(connstringAsString);
			})
				.As<ITenantUnitOfWork>()
				.As<ICurrentTenantSession>()
				.SingleInstance();
			builder.RegisterType<FindTenantLogonInfoUnsecured>().As<IFindLogonInfo>().SingleInstance();
			builder.RegisterType<TenantLogonInfoLoader>().As<ITenantLogonInfoLoader>().SingleInstance();
			builder.RegisterType<LoadAllTenants>().As<ILoadAllTenants>().SingleInstance();
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