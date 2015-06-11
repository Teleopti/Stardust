using System;
using System.Collections.Generic;
using Autofac;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.Common.Transformer
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
			builder.Register(c =>
			{
				var configReader = c.Resolve<IConfigReader>();
				var connStringToTenant = configReader.ConnectionStrings["Tenancy"];
				var connstringAsString = connStringToTenant == null ? null : connStringToTenant.ConnectionString;
				return TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(connstringAsString);
			})
				.As<ITenantUnitOfWork>()
				.As<ICurrentTenantSession>()
				.SingleInstance();
			builder.RegisterType<FindTenantLogonInfoUnsecured>().As<IFindLogonInfo>().SingleInstance();
			builder.RegisterType<TenantLogonInfoLoader>().As<ITenantLogonInfoLoader>().SingleInstance();
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
				using (_tenantUnitOfWork.Start())
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