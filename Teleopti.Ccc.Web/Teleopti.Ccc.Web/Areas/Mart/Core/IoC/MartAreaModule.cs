using System;
using Autofac;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Web.Areas.Mart.Controllers;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;

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

			// så här är cashen registrerad i RTA vi för göra ngt liknande för våra grejer
			//_configuration.Args().CacheBuilder
			//	.For<DatabaseReader>()
			//	.CacheMethod(x => x.ActivityAlarms())
			//	.CacheMethod(x => x.StateGroups())
			//	.CacheMethod(x => x.GetCurrentSchedule(Guid.NewGuid()))
			//	.CacheMethod(x => x.Datasources())
			//	.CacheMethod(x => x.ExternalLogOns())
			//	.As<IDatabaseReader>();
			//builder.RegisterMbCacheComponent<DatabaseReader, IDatabaseReader>();
		}
	}
}