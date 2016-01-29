using System;
using NUnit.Framework;
using Stardust.Node.Helpers;

namespace NodeTest
{
    [TestFixture]
    public class NodeConfigurationUriBuilderHelperTests
    {
        [Test]
        [Description("Should throw argument null exception when configuration argument is null.")]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowExceptionWhenNodeConfigurationIsNull()
        {
            var nodeConfigurationUriBuilderHelper = new NodeConfigurationUriBuilderHelper(null);
        }
    }
}