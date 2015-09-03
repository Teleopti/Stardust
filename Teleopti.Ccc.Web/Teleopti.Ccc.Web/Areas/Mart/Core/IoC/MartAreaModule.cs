using System;
using Autofac;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.Web.Areas.Mart.Core.IoC
{
	public class MartAreaModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public MartAreaModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<QueueStatHandler>().As<IQueueStatHandler>().SingleInstance();
			builder.RegisterType<QueueStatRepository>().As<IQueueStatRepository>().SingleInstance();
			builder.RegisterType<DatabaseConnectionHandler>().As<IDatabaseConnectionHandler>().SingleInstance();

			_configuration.Cache().This<IQueueStatRepository>(b => b
				.CacheMethod(x => x.GetLogObject(1, ""))
				.CacheMethod(x => x.GetQueueId("", "", 0, ""))
				.CacheMethod(x => x.GetDateId(new DateTime(), ""))
				.CacheMethod(x => x.GetIntervalLength(""))
				);
			builder.CacheByInterfaceProxy<QueueStatRepository, IQueueStatRepository>();
		}
	}
}