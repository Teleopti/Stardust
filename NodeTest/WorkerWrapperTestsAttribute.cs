using System;
using System.Configuration;
using System.Reflection;
using Autofac;
using NodeTest.Fakes;

namespace NodeTest
{
    public class WorkerWrapperTestsAttribute : BaseTestsAttribute
    {
        protected override void SetUp(ContainerBuilder builder)
        {
            // Register node configuration fake.
            var nodeConfigurationFake = new NodeConfigurationFake
            {
                BaseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"]),
                ManagerLocation = new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
                HandlerAssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"])
            };

            builder.Register(context => nodeConfigurationFake).As<NodeConfigurationFake>();
        }
    }
}