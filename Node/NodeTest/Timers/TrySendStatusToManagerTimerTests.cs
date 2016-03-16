using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using log4net;
using log4net.Config;
using NodeTest.Fakes;
using NUnit.Framework;
using Stardust.Node.API;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Timers
{
	[TestFixture]
	public class TrySendStatusToManagerTimerTests
	{
		private readonly Uri _fakeUrl = new Uri("http://localhost:9000/jobmanager/");

		private INodeConfiguration _nodeConfiguration;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			var baseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"]);

			var managerLocation = new Uri(ConfigurationManager.AppSettings["ManagerLocation"]);

			var handlerAssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]);

			var nodeName = ConfigurationManager.AppSettings["NodeName"];

			_nodeConfiguration = new NodeConfiguration(baseAddress,
			                                           managerLocation,
			                                           handlerAssembly,
			                                           nodeName,
			                                           pingToManagerSeconds: 10);

#if DEBUG
			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
#endif
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Closing TrySendStatusToManagerTimerTests...");
		}

		private static readonly ILog Logger = LogManager.GetLogger(typeof (TrySendStatusToManagerTimerTests));

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowExceptionWhenCallBackTemplateUriArgumentIsNull()
		{
			var trySendJobDoneStatusToManagerTimer = new TrySendStatusToManagerTimer(_nodeConfiguration,
			                                                                         null,
																					 null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowExceptionWhenNodeConfigurationArgumentIsNull()
		{
			var trySendJobDoneStatusToManagerTimer = new TrySendStatusToManagerTimer(null,
			                                                                         _fakeUrl,
																					 null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenSendJobProgressToManagerTimerArgumentIsNull()
		{
			var trySendJobDoneStatusToManagerTimer = new TrySendStatusToManagerTimer(_nodeConfiguration,
																					 _fakeUrl,
																					 null);
		}
	}
}