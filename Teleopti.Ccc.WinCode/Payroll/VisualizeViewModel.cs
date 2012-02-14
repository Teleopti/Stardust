using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll
{
    public class VisualizeViewModel : ViewModel<IMultiplicatorDefinition>, IVisualizeViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizeViewModel"/> class.
        /// </summary>
        /// <param name="definition">The defintion.</param>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        public VisualizeViewModel(IMultiplicatorDefinition definition)
            : base(definition)
        {
            
        }
    }
}
