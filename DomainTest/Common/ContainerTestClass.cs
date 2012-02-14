using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Testable class for Container
    /// </summary>
    public class ContainerTestClass<T> : Container<T>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerTestClass&lt;T&gt;"/> class.
        /// </summary>
        public ContainerTestClass()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerTestClass&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        public ContainerTestClass(T content) : base(content)
        {
            //
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>The content.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        public new T Content
        {
            get { return base.Content; }
            set { base.Content = value; }
        }
    }
}
