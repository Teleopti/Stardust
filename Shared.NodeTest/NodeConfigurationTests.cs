using System;
using System.Configuration;
using System.Net;
using System.Reflection;
using NUnit.Framework;
using Stardust.Node;
using Stardust.Node.Constants;
using Stardust.Node.Extensions;

namespace NodeTest
{
	[TestFixture]
	public class NodeConfigurationTests
	{
		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			NodeConfiguration = new NodeConfiguration(
				new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
				Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
				14100,
				"TestNode",
				60,
				2000, true);

			FixedIp = IPAddress.Parse("127.13.3.7");
			NodeConfigurationStaticIp = new NodeConfiguration(
				 new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
				 Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
				 1337,
				 "TestNode",
				 60,
				 2000,
				 FixedIp, true);

			UriToTest = NodeConfiguration.ManagerLocation;

			Guid = Guid.NewGuid();

			HeartBeatTemplateUri = new Uri(UriToTest, ManagerRouteConstants.Heartbeat);

			NodeHasBeenInitializedTemplateUri = new Uri(UriToTest, ManagerRouteConstants.NodeHasBeenInitialized);

			JobHasFailedTemplateUri = new Uri(UriToTest, ManagerRouteConstants.JobFailed);

			JobHasFailedUri = new Uri(JobHasFailedTemplateUri.ToString()
												  .Replace(ManagerRouteConstants.JobIdOptionalParameter,
															  Guid.ToString()));

			JobHasBeenCanceledTemplateUri = new Uri(UriToTest,
																 ManagerRouteConstants.JobHasBeenCanceled);

			JobHasBeenCanceledUri =
				new Uri(JobHasBeenCanceledTemplateUri.ToString()
							  .Replace(ManagerRouteConstants.JobIdOptionalParameter,
										  Guid.ToString()));

			JobDoneTemplateUri = new Uri(UriToTest,
												  ManagerRouteConstants.JobDone);

			JobDoneUri =
				new Uri(JobDoneTemplateUri.ToString()
							  .Replace(ManagerRouteConstants.JobIdOptionalParameter,
										  Guid.ToString()));
		}

		private NodeConfiguration NodeConfiguration { get; set; }
		private NodeConfiguration NodeConfigurationStaticIp { get; set; }
		private Uri UriToTest { get; set; }
		private Uri HeartBeatTemplateUri { get; set; }
		private Uri NodeHasBeenInitializedTemplateUri { get; set; }
		private Uri JobHasFailedTemplateUri { get; set; }
		private Uri JobHasFailedUri { get; set; }
		private Uri JobHasBeenCanceledTemplateUri { get; set; }
		private Uri JobHasBeenCanceledUri { get; set; }
		private Uri JobDoneUri { get; set; }
		private Uri JobDoneTemplateUri { get; set; }
		private Guid Guid { get; set; }
		private IPAddress FixedIp { get; set; }


		[Test]
		public void ShouldReturnCorrectGetJobDoneTemplateUri()
		{
			var uri = NodeConfiguration.GetManagerJobDoneTemplateUri();
			Assert.IsTrue(uri == JobDoneTemplateUri);
		}

		[Test]
		public void ShouldReturnCorrectGetJobHasBeenCanceledTemplateUri()
		{
			var uri = NodeConfiguration.GetManagerJobHasBeenCanceledTemplateUri();
			Assert.IsTrue(uri == JobHasBeenCanceledTemplateUri);
		}

		[Test]
		public void ShouldReturnCorrectGetJobHasFailedTemplateUri()
		{
			var uri = NodeConfiguration.GetManagerJobHasFailedTemplatedUri();
			Assert.IsTrue(uri == JobHasFailedTemplateUri);
		}

		[Test]
		public void ShouldReturnCorrectHeartBeatTemplateUri()
		{
			var uri = NodeConfiguration.GetManagerNodeHeartbeatUri();
			Assert.IsTrue(uri == HeartBeatTemplateUri);
		}

		[Test]
		public void ShouldReturnCorrectNodeHasBeenInitializedTemplateUri()
		{
			var uri = NodeConfiguration.GetManagerNodeHasBeenInitializedUri();
			Assert.IsTrue(uri == NodeHasBeenInitializedTemplateUri);
		}

		[Test]
		public void ShouldReturnCorrectUrlWhenFixedNodeIpIsSet()
		{
			var uri = NodeConfigurationStaticIp.BaseAddress;
			var expectedUri = new Uri("http://" + FixedIp + ":1337/");
			Assert.IsTrue(uri == expectedUri);
		}
	}
}