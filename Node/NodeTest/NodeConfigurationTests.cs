using System;
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
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			NodeConfiguration = new NodeConfiguration();

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
	}
}