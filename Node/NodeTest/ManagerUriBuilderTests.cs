using System;
using NUnit.Framework;
using Stardust.Node.Constants;
using Stardust.Node.Helpers;

namespace NodeTest
{
    [TestFixture]
    public class ManagerUriBuilderTests
    {
        private ManagerUriBuilderHelper ManagerUriBuilderToTest { get; set; }

        private string UriToTest { get; set; }

        private Uri HeartBeatTemplateUri { get; set; }

        private Uri NodeHasBeenInitializedTemplateUri { get; set; }

        private Uri JobHasFailedTemplateUri { get; set; }

        private Uri JobHasFailedUri { get; set; }

        private Uri JobHasBeenCanceledTemplateUri { get; set; }

        private Uri JobHasBeenCanceledeUri { get; set; }

        private Uri JobHasBeenCanceledUri { get; set; }

        private Uri JobDoneUri { get; set; }

        private Uri JobNodeTemplateUri { get; set; }

        private Guid Guid { get; set; }

        [SetUp]
        public void Setup()
        {
            Guid = Guid.NewGuid();

            UriToTest = "http://localhost:9000";

            HeartBeatTemplateUri = new Uri(new Uri(UriToTest),
                                           ManagerRouteConstants.Heartbeat);

            NodeHasBeenInitializedTemplateUri = new Uri(new Uri(UriToTest),
                                                        ManagerRouteConstants.NodeHasBeenInitialized);

            JobHasFailedTemplateUri = new Uri(new Uri(UriToTest),
                                              ManagerRouteConstants.JobFailed);

            JobHasFailedUri =
                new Uri(JobHasFailedTemplateUri.ToString()
                            .Replace(ManagerRouteConstants.JobIdOptionalParameter,
                                     Guid.ToString()));

            JobHasBeenCanceledTemplateUri = new Uri(new Uri(UriToTest),
                                                    ManagerRouteConstants.JobHasBeenCanceled);

            JobHasBeenCanceledUri =
                new Uri(JobHasBeenCanceledTemplateUri.ToString()
                            .Replace(ManagerRouteConstants.JobIdOptionalParameter,
                                     Guid.ToString()));

            JobNodeTemplateUri = new Uri(new Uri(UriToTest),
                                         ManagerRouteConstants.JobDone);

            JobDoneUri =
                new Uri(JobNodeTemplateUri.ToString()
                            .Replace(ManagerRouteConstants.JobIdOptionalParameter,
                                     Guid.ToString()));
        }


        [Test]
        [ExpectedException(typeof (System.UriFormatException))]
        public void ShouldThrowExceptionWhenConstructorArgumentIsStringEmpty()
        {
            ManagerUriBuilderToTest = new ManagerUriBuilderHelper(string.Empty);
        }

        [Test]
        [ExpectedException(typeof (System.UriFormatException))]
        public void ShouldThrowExceptionWhenConstructorArgumentStringIsIvalidUri()
        {
            ManagerUriBuilderToTest = new ManagerUriBuilderHelper("invalid uri");
        }

        [Test]
        public void ShouldInstantiateWhenConstructorArgumentStringIsIvalidUri()
        {
            ManagerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsNotNull(ManagerUriBuilderToTest);
        }

        [Test]
        public void ShouldReturnCorrectUriInformation()
        {
            var uri = new Uri(UriToTest);

            ManagerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(ManagerUriBuilderToTest.GetHostName() == uri.Host);
            Assert.IsTrue(ManagerUriBuilderToTest.GetScheme() == uri.Scheme);
            Assert.IsTrue(ManagerUriBuilderToTest.GetPort() == uri.Port);
            Assert.IsTrue(ManagerUriBuilderToTest.GetLocationUri() == uri);
        }

        [Test]
        public void ShouldReturnCorrectHeartBeatTemplateUri()
        {
            ManagerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(ManagerUriBuilderToTest.GetHeartbeatTemplateUri() == HeartBeatTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectNodeHasBeenInitializedTemplateUri()
        {
            ManagerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(ManagerUriBuilderToTest.GetNodeHasBeenInitializedTemplateUri() == NodeHasBeenInitializedTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobHasFailedTemplateUri()
        {
            ManagerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(ManagerUriBuilderToTest.GetJobHasFailedTemplateUri() == JobHasFailedTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobHasBeenCanceledTemplateUri()
        {
            ManagerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(ManagerUriBuilderToTest.GetJobHasBeenCanceledTemplateUri() == JobHasBeenCanceledTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobDoneTemplateUri()
        {
            ManagerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(ManagerUriBuilderToTest.GetJobDoneTemplateUri() == JobNodeTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobHasBeenCanceledUri()
        {
            ManagerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(ManagerUriBuilderToTest.GetJobHasBeenCanceledUri(Guid) == JobHasBeenCanceledUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobDoneUri()
        {
            ManagerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(ManagerUriBuilderToTest.GetJobDoneUri(Guid) == JobDoneUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobHasFailedUri()
        {
            ManagerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(ManagerUriBuilderToTest.GetJobHasFailedUri(Guid) == JobHasFailedUri);
        }
    }
}