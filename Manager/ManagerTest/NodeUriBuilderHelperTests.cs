using System;
using NUnit.Framework;
using Stardust.Manager.Constants;
using Stardust.Manager.Helpers;

namespace ManagerTest
{
    [TestFixture]
    public class NodeUriBuilderHelperTests
    {
        private NodeUriBuilderHelper NodeUriBuilderHelperToTest { get; set; }

        private UriBuilder UriBuilder { get; set; }

        private Guid Guid { get; set; }

        private string UriToTest { get; set; }

        private Uri JobTemplateUri { get; set; }

        private Uri CancelJobTemplateUri { get; set; }

        private Uri CancelJobUri { get; set; }

        private Uri CancelJobTUri { get; set; }

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

            UriBuilder.Path = NodeRouteConstants.CancelJob;
            CancelJobTemplateUri = UriBuilder.Uri;

            UriBuilder.Path =
                NodeRouteConstants.CancelJob.Replace(NodeRouteConstants.JobIdOptionalParameter,
                                                     Guid.ToString());

            CancelJobUri = UriBuilder.Uri;
        }

        [Test]
        [ExpectedException(typeof (System.UriFormatException))]
        public void ShouldThrowExceptionWhenConstructorArgumentIsStringEmpty()
        {
            NodeUriBuilderHelperToTest = new NodeUriBuilderHelper(string.Empty);
        }

        [Test]
        [ExpectedException(typeof (System.UriFormatException))]
        public void ShouldThrowExceptionWhenConstructorArgumentStringIsIvalidUri()
        {
            NodeUriBuilderHelperToTest = new NodeUriBuilderHelper("invalid uri");
        }

        [Test]
        public void ShouldInstantiateWhenConstructorArgumentStringIsIvalidUri()
        {
            NodeUriBuilderHelperToTest = new NodeUriBuilderHelper(UriToTest);

            Assert.IsNotNull(NodeUriBuilderHelperToTest);
        }

        [Test]
        public void ShouldReturnCorrectUriInformation()
        {
            var uri = new Uri(UriToTest);

            NodeUriBuilderHelperToTest = new NodeUriBuilderHelper(UriToTest);

            Assert.IsTrue(NodeUriBuilderHelperToTest.GetHostName() == uri.Host);
            Assert.IsTrue(NodeUriBuilderHelperToTest.GetScheme() == uri.Scheme);
            Assert.IsTrue(NodeUriBuilderHelperToTest.GetPort() == uri.Port);
            Assert.IsTrue(NodeUriBuilderHelperToTest.GetLocationUri() == uri);
        }

        [Test]
        public void ShouldReturnCorrectJobTemplateUri()
        {
            NodeUriBuilderHelperToTest = new NodeUriBuilderHelper(UriToTest);

            Assert.IsTrue(NodeUriBuilderHelperToTest.GetJobTemplateUri() == JobTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectCancelJobTemplateUri()
        {
            NodeUriBuilderHelperToTest = new NodeUriBuilderHelper(UriToTest);

            Assert.IsTrue(NodeUriBuilderHelperToTest.GetCancelJobTemplateUri() == CancelJobTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectCancelJobUri()
        {
            NodeUriBuilderHelperToTest = new NodeUriBuilderHelper(UriToTest);

            Assert.IsTrue(NodeUriBuilderHelperToTest.GetCancelJobUri(Guid) == CancelJobUri);
        }
    }
}