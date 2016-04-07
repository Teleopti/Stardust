using System;
using System.Configuration;
using System.Reflection;
using NUnit.Framework;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

namespace NodeTest.Timers
{
	[TestFixture]
	public class TrySendStatusToManagerTimerTests
	{
		private readonly Uri _fakeUrl = new Uri("http://localhost:9000/jobmanager/");

		private NodeConfiguration _nodeConfiguration;

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
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowExceptionWhenCallBackTemplateUriArgumentIsNull()
		{
			new TrySendStatusToManagerTimer(_nodeConfiguration,
				null,
				null,
				null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowExceptionWhenNodeConfigurationArgumentIsNull()
		{
			new TrySendStatusToManagerTimer(null,
				_fakeUrl,
				null,
				null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowExceptionWhenSendJobProgressToManagerTimerArgumentIsNull()
		{
			new TrySendStatusToManagerTimer(_nodeConfiguration,
				_fakeUrl,
				null,
				null);
		}
	}
}