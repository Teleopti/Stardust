using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Annotatable Entity.
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2008-10-14
    /// </remarks>
    public interface IAnnotatable
    {
        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        /// <value>The annotation.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-10-14
        /// </remarks>
        string Annotation
        {
            get; set;
        }
    }
}
