using System;
using System.IO;
using System.Threading;
using Autofac;
using log4net;
using log4net.Config;
using NodeTest.JobHandlers;
using NUnit.Framework;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace NodeTest
{
    [TestFixture]
    public class JobHandlersTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance<IHandle<TestJobParams>>(new TestJobWorker(new TestJobCode()));

            builder.RegisterType<InvokeHandler>()
                .SingleInstance();

            Container = builder.Build();

#if DEBUG
            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
#endif

        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.LogInfoWithLineNumber(Logger, "Closing JobHandlersTests...");
        }

        private IContainer Container { get; set; }
        private static readonly ILog Logger = LogManager.GetLogger(typeof(JobHandlersTests));

        private void ProgressCallback(string message)
        {
           LogHelper.LogInfoWithLineNumber(Logger, message);
        }

        [Test]
        public void ShouldBeAbleToInvokeAHandler()
        {
            var invokehandler = Container.Resolve<InvokeHandler>();

            invokehandler.Invoke(new TestJobParams("Dummy data",
                                                   "Name data"),
                                 new CancellationTokenSource(),
                                 ProgressCallback);
        }

        [Test]
        public void ShouldBeAbleToResolveAHandlerFromContainer()
        {
            var handler = Container.Resolve<IHandle<TestJobParams>>();

            Assert.IsNotNull(handler);
        }

        [Test]
        public void ShouldBeAbleToResolveInvokeHandlerFromContainer()
        {
            var invokehandler = Container.Resolve<InvokeHandler>();

            Assert.IsNotNull(invokehandler);
        }
    }
}