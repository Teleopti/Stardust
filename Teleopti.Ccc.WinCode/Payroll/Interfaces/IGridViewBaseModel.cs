﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll.Interfaces
{
    public interface IGridViewBaseModel
    {
        /// <summary>
        /// Gets or sets the multiplicator collection.
        /// </summary>
        /// <value>The multiplicator collection.</value>
        IList<IMultiplicator> MultiplicatorCollection{ get; set;}

        /// <summary>
        /// Gets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        IMultiplicator Multiplicator { get; set; }
      
    }
}
