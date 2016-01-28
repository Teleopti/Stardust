using NUnit.Framework;
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


    }
}