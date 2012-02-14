using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Interface for muliplicators as Ob and Extra time
    ///</summary>
    public interface IMultiplicator : IAggregateRoot, ICloneableEntity<IMultiplicator>
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-08
        /// </remarks>
        Description Description { get; set; }

        /// <summary>
        /// Gets or sets the display color.
        /// </summary>
        /// <value>The display color.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-08
        /// </remarks>
        Color DisplayColor { get; set; }

        /// <summary>
        /// Gets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-08
        /// </remarks>
        MultiplicatorType MultiplicatorType { get; set; }

        /// <summary>
        /// Gets or sets the multiplicator value.
        /// </summary>
        /// <value>The multiplicator value.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-08
        /// </remarks>
        double MultiplicatorValue { get; set; }

        /// <summary>
        /// Gets or sets the export code. Typicaly used by pay roll reporting
        /// </summary>
        /// <value>The export code.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-08
        /// </remarks>
        string ExportCode { get; set; }

    }
}
