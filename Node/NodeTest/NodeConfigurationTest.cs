using System;
using NUnit.Framework;
using Stardust.Node.API;

namespace NodeTest
{
    internal class NodeConfigurationTest
    {
        private NodeConfiguration _nodeConfiguration;

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowExceptionWhenConstructorArgumentIsNull()
        {
            _nodeConfiguration = new NodeConfiguration(null,
                                                       null,
                                                       null,
                                                       null);
        }
    }
}