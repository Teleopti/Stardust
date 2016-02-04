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

        [SetUp]
        public void Setup()
        {
            UriToTest = "http://localhost:9000/jobmanager/";
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowExceptionWhenConstructorArgumentIsStringNull()
        {
            var nodeUriBuilderToTest = new NodeUriBuilderHelper(location: null);
        }

        [Test]
        [ExpectedException(typeof (System.ArgumentNullException))]
        public void ShouldThrowExceptionWhenConstructorArgumentIsStringEmpty()
        {
            var nodeUriBuilderToTest = new NodeUriBuilderHelper(string.Empty);
        }

        [Test]
        [ExpectedException(typeof (System.UriFormatException))]
        public void ShouldThrowExceptionWhenConstructorArgumentStringIsIvalidUri()
        {
            var nodeUriBuilderToTest = new NodeUriBuilderHelper("invalid uri");
        }

        [Test]
        public void ShouldInstantiateWhenConstructorArgumentStringIsIvalidUri()
        {
            var nodeUriBuilderToTest = new NodeUriBuilderHelper(UriToTest);

            Assert.IsNotNull(nodeUriBuilderToTest);
        }

        [Test]
        public void ShouldReturnCorrectUriWhenPathArgumentIsAssigned()
        {
            string path = NodeRouteConstants.IsAlive;

            var uriBuilder = new UriBuilder(UriToTest)
            {
                Path = path
            };

            var nodeUriBuilderToTest = new NodeUriBuilderHelper(UriToTest);

            var uriToTest = nodeUriBuilderToTest.CreateUri(path);

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

            var nodeUriBuilderToTest = new NodeUriBuilderHelper(UriToTest);

            var uriToTest = nodeUriBuilderToTest.CreateUri(NodeRouteConstants.CancelJob,
                                                           guid);

            Assert.IsTrue(uriBuilder.Uri == uriToTest);
        }
    }
}