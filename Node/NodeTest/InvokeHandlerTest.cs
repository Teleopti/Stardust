using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Node.Interfaces;
using Autofac;
using node.Workers;


namespace NodeTest
{
    [TestFixture]
    class InvokeHandlerTest
    {
        private InvokeHandler _invokeHandler;
        private IComponentContext _container;


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionWhenConstructorArgumentIsNull()
        {
            _invokeHandler = new InvokeHandler(_container);
        }

    }
}
