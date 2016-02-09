using System;
using NUnit.Framework;
using Stardust.Node.Constants;
using Stardust.Node.Helpers;

namespace NodeTest
{
    [TestFixture]
    public class ManagerUriBuilderTests
    {
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

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Guid = Guid.NewGuid();

            UriToTest = "http://localhost:9000/jobmanager/";

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionWhenConstructorArgumentIsStringNull()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper(location:null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowExceptionWhenConstructorArgumentIsStringEmpty()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper(string.Empty);
        }

        [Test]
        [ExpectedException(typeof (UriFormatException))]
        public void ShouldThrowExceptionWhenConstructorArgumentStringIsIvalidUri()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper("invalid uri");
        }

        [Test]
        public void ShouldInstantiateWhenConstructorArgumentStringIsIvalidUri()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsNotNull(managerUriBuilderToTest);
        }

        [Test]
        public void ShouldReturnCorrectUriInformation()
        {
            var uriBuilder = new UriBuilder(UriToTest);

            var managerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(managerUriBuilderToTest.GetHostName() == uriBuilder.Host);
            Assert.IsTrue(managerUriBuilderToTest.GetScheme() == uriBuilder.Scheme);
            Assert.IsTrue(managerUriBuilderToTest.GetPort() == uriBuilder.Port);
            Assert.IsTrue(managerUriBuilderToTest.GetLocationUri() == uriBuilder.Uri);
        }

        [Test]
        public void ShouldReturnCorrectHeartBeatTemplateUri()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(managerUriBuilderToTest.GetHeartbeatTemplateUri() == HeartBeatTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectNodeHasBeenInitializedTemplateUri()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(managerUriBuilderToTest.GetNodeHasBeenInitializedTemplateUri() == NodeHasBeenInitializedTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobHasFailedTemplateUri()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(managerUriBuilderToTest.GetJobHasFailedTemplateUri() == JobHasFailedTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobHasBeenCanceledTemplateUri()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(managerUriBuilderToTest.GetJobHasBeenCanceledTemplateUri() == JobHasBeenCanceledTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobDoneTemplateUri()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(managerUriBuilderToTest.GetJobDoneTemplateUri() == JobNodeTemplateUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobHasBeenCanceledUri()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(managerUriBuilderToTest.GetJobHasBeenCanceledUri(Guid) == JobHasBeenCanceledUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobDoneUri()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(managerUriBuilderToTest.GetJobDoneUri(Guid) == JobDoneUri);
        }

        [Test]
        public void ShouldReturnCorrectGetJobHasFailedUri()
        {
            var managerUriBuilderToTest = new ManagerUriBuilderHelper(UriToTest);

            Assert.IsTrue(managerUriBuilderToTest.GetJobHasFailedUri(Guid) == JobHasFailedUri);
        }
    }
}