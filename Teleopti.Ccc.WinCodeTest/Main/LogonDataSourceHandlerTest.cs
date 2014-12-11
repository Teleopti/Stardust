using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.WinCodeTest.Main
{
    [TestFixture]
    public class LogonDataSourceHandlerTest
    {
         [Test]
         public void ShouldResolve()
         {
             var builder = new ContainerBuilder();
             builder.RegisterModule(CommonModule.ForTest());
             builder.RegisterType<LogonDataSourceHandler>().As<IDataSourceHandler>();
             builder.RegisterType<EnvironmentWindowsUserProvider>()
                   .As<IWindowsUserProvider>()
                   .SingleInstance();
             var container = builder.Build();
             var target = container.Resolve<IDataSourceHandler>();
             var res = target.AvailableDataSourcesProvider();
             Assert.That(res,Is.Not.Null);

             var providers = target.DataSourceProviders();
             Assert.That(providers, Is.Not.Null);
         }
    }
}