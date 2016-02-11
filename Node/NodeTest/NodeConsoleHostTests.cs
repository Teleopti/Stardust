using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NUnit.Framework;
using Stardust.Node.Helpers;

namespace NodeTest
{
    [TestFixture()]
    [Ignore]
    public class NodeConsoleHostTests
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (NodeConsoleHostTests));

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Start TestFixtureTearDown");

            string friendlyName = MyAppDomain.FriendlyName;

            try
            {
                LogHelper.LogInfoWithLineNumber(Logger,
                                                "Try unload appdomain with friendly name : " + friendlyName);

                AppDomain.Unload(MyAppDomain);

                LogHelper.LogInfoWithLineNumber(Logger,
                                                "Unload appdomain with friendly name : " + friendlyName);
            }

            catch (AppDomainUnloadedException appDomainUnloadedException)
            {
                LogHelper.LogWarningWithLineNumber(Logger,
                                                   appDomainUnloadedException.Message);
            }

            catch (Exception exp)
            {
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 exp.Message,
                                                 exp);
            }

            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Finished TestFixtureTearDown");
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            MyTask = new Task(() =>
            {
                var path =
                    @"C:\Users\antons\Documents\Stardust\Node\NodeConsoleHost\bin\Debug";

                var exe = "NodeConsoleHost.exe";

                var config = "NodeConsoleHost.exe.config";

                var appDomainSetup = new AppDomainSetup
                {
                    ApplicationBase = path,
                    ApplicationName = exe,
                    ConfigurationFile = config
                };

                MyAppDomain = AppDomain.CreateDomain(exe,
                                                     null,
                                                     appDomainSetup);


                MyAppDomain.ExecuteAssembly(Path.Combine(path,
                                                         exe));
            });

            MyTask.Start();

            Thread.Sleep(TimeSpan.FromSeconds(60));
        }

        public Task MyTask { get; set; }

        [Test]
        public void Test1()
        {
            Assert.IsTrue(true);
        }

        public AppDomain MyAppDomain { get; set; }
    }
}