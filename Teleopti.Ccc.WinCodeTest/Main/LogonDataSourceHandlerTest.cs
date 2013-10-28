using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Main
{
    [TestFixture]
    public class LogonDataSourceHandlerTest
    {
         [Test]
         public void ShouldResolve()
         {
             var containerBuilder = new ContainerBuilder();
             containerBuilder.RegisterModule(new AuthenticationModule());
             containerBuilder.RegisterType<LogonDataSourceHandler>().As<IDataSourceHandler>();
             containerBuilder.RegisterType<EnvironmentWindowsUserProvider>()
                   .As<IWindowsUserProvider>()
                   .SingleInstance();
             var container = containerBuilder.Build();
             var target = container.Resolve<IDataSourceHandler>();
             var res = target.AvailableDataSourcesProvider();
             Assert.That(res,Is.Not.Null);

             var providers = target.DataSourceProviders();
             Assert.That(providers, Is.Not.Null);
         }
    }
}