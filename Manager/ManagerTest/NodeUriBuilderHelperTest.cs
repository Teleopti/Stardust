using System;
using NUnit.Framework;
using Stardust.Manager.Constants;
using Stardust.Manager.Helpers;

namespace ManagerTest
{
	[TestFixture]
	public class NodeUriBuilderHelperTest
	{
		[SetUp]
		public void Setup()
		{
			Guid = Guid.NewGuid();

			UriToTest = "http://localhost:9000";

			UriBuilder = new UriBuilder(UriToTest)
			{
				Path = NodeRouteConstants.Job
			};

			JobTemplateUri = UriBuilder.Uri;

			UriBuilder.Path = NodeRouteConstants.CancelJobByJobId;
			CancelJobTemplateUri = UriBuilder.Uri;

			UriBuilder.Path =
				NodeRouteConstants.CancelJobByJobId.Replace(NodeRouteConstants.JobIdOptionalParameter,
				                                     Guid.ToString());

			CancelJobUri = UriBuilder.Uri;
		}

		private NodeUriBuilderHelper NodeUriBuilderHelperToTest { get; set; }

		private UriBuilder UriBuilder { get; set; }

		private Guid Guid { get; set; }

		private string UriToTest { get; set; }

		private Uri JobTemplateUri { get; set; }

		private Uri CancelJobTemplateUri { get; set; }

		private Uri CancelJobUri { get; set; }

		[Test]
		public void ShouldInstantiateWhenConstructorArgumentStringIsIvalidUri()
		{
			NodeUriBuilderHelperToTest = new NodeUriBuilderHelper(UriToTest);

			Assert.IsNotNull(NodeUriBuilderHelperToTest);
		}

		[Test]
		public void ShouldReturnCorrectCancelJobUri()
		{
			NodeUriBuilderHelperToTest = new NodeUriBuilderHelper(UriToTest);

			Assert.IsTrue(NodeUriBuilderHelperToTest.GetCancelJobUri(Guid) == CancelJobUri);
		}

		[Test]
		public void ShouldReturnCorrectJobTemplateUri()
		{
			NodeUriBuilderHelperToTest = new NodeUriBuilderHelper(UriToTest);

			Assert.IsTrue(NodeUriBuilderHelperToTest.GetJobTemplateUri() == JobTemplateUri);
		}

		[Test]
		public void ShouldThrowExceptionWhenConstructorArgumentIsStringEmpty()
		{
			Assert.Throws<UriFormatException>(() => NodeUriBuilderHelperToTest = new NodeUriBuilderHelper(string.Empty));
		}

		[Test]
		public void ShouldThrowExceptionWhenConstructorArgumentStringIsIvalidUri()
		{
			Assert.Throws<UriFormatException>(() => NodeUriBuilderHelperToTest = new NodeUriBuilderHelper("invalid uri"));
		}
	}
}