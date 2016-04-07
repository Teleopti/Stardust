using System;
using NUnit.Framework;
using Stardust.Node.Constants;
using Stardust.Node.Helpers;

namespace NodeTest
{
	[TestFixture]
	public class NodeUriBuilderTests
	{
		private string uriToTest = "http://localhost:9000/jobmanager/";

		[Test]
		public void ShouldInstantiateWhenConstructorArgumentStringIsIvalidUri()
		{
			var nodeUriBuilderToTest = new NodeUriBuilderHelper(uriToTest);

			Assert.IsNotNull(nodeUriBuilderToTest);
		}

		[Test]
		public void ShouldReturnCorrectUriWhenPathArgumentIsAssigned()
		{
			var path = NodeRouteConstants.IsAlive;

			var uriBuilder = new UriBuilder(uriToTest)
			{
				Path = path
			};

			var nodeUriBuilderToTest = new NodeUriBuilderHelper(uriToTest);

			var uri = nodeUriBuilderToTest.CreateUri(path);

			Assert.IsTrue(uriBuilder.Uri == uri);
		}

		[Test]
		public void ShouldReturnCorrectUriWhenTemplateAndGuidArgumentsAreAssigned()
		{
			var guid = Guid.NewGuid();

			var path =
				NodeRouteConstants.CancelJob.Replace(NodeRouteConstants.JobIdOptionalParameter,
				                                     guid.ToString());

			var uriBuilder = new UriBuilder(uriToTest)
			{
				Path = path
			};

			var nodeUriBuilderToTest = new NodeUriBuilderHelper(uriToTest);

			var uri = nodeUriBuilderToTest.CreateUri(NodeRouteConstants.CancelJob,
			                                               guid);

			Assert.IsTrue(uriBuilder.Uri == uri);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowExceptionWhenConstructorArgumentIsStringEmpty()
		{
			new NodeUriBuilderHelper(string.Empty);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowExceptionWhenConstructorArgumentIsStringNull()
		{
			new NodeUriBuilderHelper(location: null);
		}

		[Test]
		[ExpectedException(typeof (UriFormatException))]
		public void ShouldThrowExceptionWhenConstructorArgumentStringIsIvalidUri()
		{
			new NodeUriBuilderHelper("invalid uri");
		}
	}
}