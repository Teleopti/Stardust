using System;
using NUnit.Framework;
using Stardust.Node.Constants;
using Stardust.Node.Helpers;

namespace NodeTest
{
    [TestFixture]
    public class NodeUriBuilderTests
    {
        private string UriToTest { get; set; }

        private NodeUriBuilder NodeUriBuilderToTest { get; set; }

        [SetUp]
        public void Setup()
        {
            UriToTest = "http://localhost:9000";
        }

        [Test]
        [ExpectedException(typeof (System.UriFormatException))]
        public void ShouldThrowExceptionWhenConstructorArgumentIsStringEmpty()
        {
            NodeUriBuilderToTest = new NodeUriBuilder(string.Empty);
        }

        [Test]
        [ExpectedException(typeof (System.UriFormatException))]
        public void ShouldThrowExceptionWhenConstructorArgumentStringIsIvalidUri()
        {
            NodeUriBuilderToTest = new NodeUriBuilder("invalid uri");
        }

        [Test]
        public void ShouldInstantiateWhenConstructorArgumentStringIsIvalidUri()
        {
            NodeUriBuilderToTest = new NodeUriBuilder(UriToTest);

            Assert.IsNotNull(NodeUriBuilderToTest);
        }

        [Test]
        public void ShouldReturnCorrectUriWhenPathArgumentIsAssigned()
        {
            string path = NodeRouteConstants.IsAlive;

            var uriBuilder = new UriBuilder(UriToTest)
            {
                Path = path
            };

            NodeUriBuilderToTest = new NodeUriBuilder(UriToTest);

            var uriToTest = NodeUriBuilderToTest.CreateUri(path);

            Assert.IsTrue(uriBuilder.Uri == uriToTest);
        }

        [Test]
        public void ShouldReturnCorrectUriWhenTemplateAndGuidArgumentsAreAssigned()
        {
            var guid = Guid.NewGuid();

            string path =
                NodeRouteConstants.CancelJob.Replace(NodeRouteConstants.JobIdOptionalParameter,
                                                     guid.ToString());

            var uriBuilder = new UriBuilder(UriToTest)
            {
                Path = path
            };

            NodeUriBuilderToTest = new NodeUriBuilder(UriToTest);

            var uriToTest = NodeUriBuilderToTest.CreateUri(NodeRouteConstants.CancelJob,
                                                           guid);

            Assert.IsTrue(uriBuilder.Uri == uriToTest);
        }
    }
}