﻿using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class IoCTest
	{
		[Test]
		public void CanResolveTenantOfWorkAspect()
		{
			using (var container = buildContainer())
			{
				container.Resolve<ITenantUnitOfWorkAspect>()
					.Should().Not.Be.Null();
			}
		}

		private static IContainer buildContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new WebAppModule(new IocConfiguration(new IocArgs(new AppConfigReader()), null)));

			return builder.Build();
		}
	}
}